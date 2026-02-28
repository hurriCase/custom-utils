using System;
using System.Collections.Generic;
using System.IO;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Runtime.Extensions;
using UnityEditor;
using UnityEngine;
using static UnityEngine.RenderTexture;

namespace CustomUtils.Editor.Scripts.SpriteFix
{
    internal sealed class SpriteResizerWindow : WindowBase
    {
        private Mode _currentMode = Mode.SingleSprite;
        private Sprite _selectedSprite;
        private DefaultAsset _selectedFolder;
        private Vector2 _scrollPosition;
        private bool _showPreview = true;
        private readonly List<SpriteResizeInfo> _spritesToResize = new();
        private bool _processingComplete;

        [MenuItem(MenuItemNames.FixNPOTMenuName)]
        internal static void ShowWindow()
        {
            GetWindow<SpriteResizerWindow>(nameof(SpriteResizerWindow).ToSpacedWords());
        }

        protected override void InitializeWindow()
        {
            _showPreview = true;
            _processingComplete = false;
        }

        protected override void DrawWindowContent()
        {
            DrawSection("Mode", DrawModeSelection);

            switch (_currentMode)
            {
                case Mode.SingleSprite:
                    DrawSection("Single Sprite Mode", DrawSingleSpriteMode);
                    break;

                case Mode.Folder:
                    DrawSection("Folder Mode", DrawFolderMode);
                    break;

                case Mode.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawModeSelection()
        {
            _currentMode = EditorStateControls.EnumField(_currentMode);
        }

        private void DrawSingleSpriteMode()
        {
            EditorGUI.BeginChangeCheck();
            _selectedSprite = EditorStateControls.SpriteField("Select Sprite", _selectedSprite);

            if (EditorGUI.EndChangeCheck() && _selectedSprite)
            {
                _spritesToResize.Clear();
                AddSpriteToList(_selectedSprite);
                _processingComplete = false;
            }

            if (!_selectedSprite)
                return;

            DrawSpriteList();

            EditorGUILayout.Space();

            if (GUILayout.Button("Resize Sprite", GUILayout.Height(30)))
                ProcessSprites(_spritesToResize);
        }

        private void DrawFolderMode()
        {
            EditorGUI.BeginChangeCheck();
            _selectedFolder = (DefaultAsset)EditorStateControls
                .ObjectField("Select Folder", _selectedFolder, typeof(DefaultAsset));

            if (EditorGUI.EndChangeCheck() && _selectedFolder)
            {
                var folderPath = AssetDatabase.GetAssetPath(_selectedFolder);
                if (Directory.Exists(folderPath))
                {
                    FindSpritesInFolder(folderPath);
                    _processingComplete = false;
                }
                else
                {
                    _selectedFolder = null;
                    EditorUtility.DisplayDialog("Error", "Please select a valid folder.", "OK");
                }
            }

            if (!_selectedFolder)
                return;

            _showPreview = EditorStateControls.Toggle("Show Preview", _showPreview);

            if (_showPreview)
                DrawSpriteList();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh", GUILayout.Height(25)))
            {
                var folderPath = AssetDatabase.GetAssetPath(_selectedFolder);
                FindSpritesInFolder(folderPath);
                _processingComplete = false;
            }

            if (GUILayout.Button("Resize All Sprites", GUILayout.Height(25)))
                ProcessSprites(_spritesToResize);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSpriteList()
        {
            EditorGUILayout.Space();
            EditorVisualControls.DrawSectionHeader("Sprites to process:");

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var spriteInfo in _spritesToResize)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Sprite: " + Path.GetFileName(spriteInfo.Path));
                EditorGUILayout.LabelField("Original Size: " + spriteInfo.OriginalSize.x + " x " +
                                           spriteInfo.OriginalSize.y);
                EditorGUILayout.LabelField("New Size: " + spriteInfo.NewSize.x + " x " + spriteInfo.NewSize.y);

                if (!spriteInfo.ShouldResize)
                    EditorGUILayout.LabelField("(No resize needed)", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();

            if (_processingComplete)
                EditorVisualControls.WarningBox("Processing complete!");
        }

        private void FindSpritesInFolder(string folderPath)
        {
            _spritesToResize.Clear();
            var guids = AssetDatabase.FindAssets("t:sprite", new[] { folderPath });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite)
                    AddSpriteToList(sprite);
            }
        }

        private void AddSpriteToList(Sprite sprite)
        {
            var originalSize = new Vector2(sprite.texture.width, sprite.texture.height);
            var newSize = new Vector2(
                RoundToNearestMultipleOf4((int)originalSize.x),
                RoundToNearestMultipleOf4((int)originalSize.y)
            );

            _spritesToResize.Add(new SpriteResizeInfo
            {
                Path = AssetDatabase.GetAssetPath(sprite),
                Sprite = sprite,
                OriginalSize = originalSize,
                NewSize = newSize
            });
        }

        private int RoundToNearestMultipleOf4(int value)
            => Mathf.CeilToInt(value / 4f) * 4;

        private void ProcessSprites(List<SpriteResizeInfo> spriteInfos)
        {
            var processedCount = 0;
            var totalToProcess = spriteInfos.Count(static spriteResizeInfo => spriteResizeInfo.ShouldResize);

            foreach (var spriteInfo in spriteInfos)
            {
                if (!spriteInfo.ShouldResize)
                    continue;

                EditorUtility.DisplayProgressBar(
                    "Processing Sprites",
                    $"Processing {Path.GetFileName(spriteInfo.Path)} ({processedCount + 1}/{totalToProcess})",
                    (float)processedCount / totalToProcess
                );

                ResizeSprite(spriteInfo);
                processedCount++;
            }

            EditorUtility.ClearProgressBar();
            _processingComplete = true;

            EditorUtility.DisplayDialog("Sprite Resizer",
                processedCount > 0
                    ? $"Successfully processed {processedCount} sprite{(processedCount != 1 ? "s" : "")}!"
                    : "No sprites needed resizing!",
                "OK");
        }

        private void ResizeSprite(SpriteResizeInfo spriteInfo)
        {
            var sourceTexture = spriteInfo.Sprite.texture;

            var renderTexture = new RenderTexture(
                (int)spriteInfo.NewSize.x,
                (int)spriteInfo.NewSize.y,
                0);

            active = renderTexture;
            Graphics.Blit(sourceTexture, renderTexture);

            var newTexture = new Texture2D(
                (int)spriteInfo.NewSize.x,
                (int)spriteInfo.NewSize.y);

            newTexture.ReadPixels(
                new Rect(0, 0, (int)spriteInfo.NewSize.x, (int)spriteInfo.NewSize.y), 0, 0);

            newTexture.Apply();
            active = null;
            renderTexture.Release();

            var bytes = newTexture.EncodeToPNG();
            File.WriteAllBytes(spriteInfo.Path, bytes);
            AssetDatabase.ImportAsset(spriteInfo.Path);

            if (AssetImporter.GetAtPath(spriteInfo.Path) is not TextureImporter importer)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }
}