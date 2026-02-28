using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.SpriteFix
{
    internal sealed class SpriteAlphaAdderWindow : WindowBase
    {
        private Sprite _targetSprite;
        private Vector2 _scrollPosition;
        private readonly List<TextureImporter> _problematicSprites = new();
        private bool _showProblematicSprites;

        [MenuItem(MenuItemNames.SpriteAlphaAdderMenuName)]
        internal static void ShowWindow()
        {
            GetWindow<SpriteAlphaAdderWindow>(nameof(SpriteAlphaAdderWindow).ToSpacedWords());
        }

        protected override void DrawWindowContent()
        {
            DrawProgressIfNeeded();

            GUI.enabled = !EditorProgressTracker.HasOperation;

            DrawSection("Project Scanner", DrawScan);
            DrawSection("Manual Sprite Processing", DrawManualSelection);

            GUI.enabled = true;
        }

        private void DrawScan()
        {
            if (EditorVisualControls.Button("Scan Project for Problematic Sprites"))
                FindProblematicSpritesAsync().Forget();

            if (_problematicSprites.Count <= 0)
                return;

            EditorVisualControls.Foldout(
                $"Problematic Sprites Found: {_problematicSprites.Count}",
                ref _showProblematicSprites,
                DrawProblematicSprites);
        }

        private void DrawProblematicSprites()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorVisualControls.DrawPanel(() =>
            {
                foreach (var importer in _problematicSprites)
                    DrawSpriteEntry(importer);
            });

            EditorGUILayout.EndScrollView();

            if (EditorVisualControls.Button("Fix All", GUILayout.Height(25)))
                FixAllSpritesAsync().Forget();
        }

        private void DrawSpriteEntry(TextureImporter importer)
        {
            EditorGUILayout.BeginHorizontal();

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);
            if (!texture)
                return;

            EditorVisualControls.Label(AssetPreview.GetAssetPreview(texture),
                GUILayout.Width(50), GUILayout.Height(50));

            EditorVisualControls.LabelField(Path.GetFileName(importer.assetPath));
            EditorVisualControls.LabelField($"Size: {texture.width}x{texture.height}", EditorStyles.miniLabel);

            if (EditorVisualControls.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = texture;
                EditorGUIUtility.PingObject(texture);
            }

            if (EditorVisualControls.Button("Fix", GUILayout.Width(40)))
                AddAlphaPixelAsync(importer).Forget();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawManualSelection()
        {
            _targetSprite = EditorStateControls.SpriteField("Target Sprite", _targetSprite);

            if (!_targetSprite)
                return;

            var path = AssetDatabase.GetAssetPath(_targetSprite);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            var isRGBA = importer && importer.DoesSourceTextureHaveAlpha();

            EditorVisualControls.H3Label($"Current Format: {(isRGBA ? "RGBA" : "RGB")}");

            if (!isRGBA)
                EditorVisualControls.DrawPanel(() =>
                {
                    if (EditorVisualControls.Button("Add Alpha Pixel", GUILayout.Height(25)))
                        AddAlphaPixelAsync(importer).Forget();
                });
            else
                EditorVisualControls.WarningBox("This sprite already has an alpha channel.");
        }

        private async UniTaskVoid FindProblematicSpritesAsync() =>
            await EditorProgressTracker.CreateProgressAsync(
                "Scanning Project",
                "Searching for problematic sprites...",
                ScanForProblematicSprites);

        private async UniTask<string> ScanForProblematicSprites(
            IProgress<float> progress, CancellationToken cancellationToken)
        {
            _problematicSprites.Clear();
            var guids = AssetDatabase.FindAssets("t:texture2d");
            var totalCount = guids.Length;
            var processedCount = 0;

            foreach (var guid in guids)
            {
                await UniTask.Yield(cancellationToken);

                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsProblematicSprite(path, out var importer))
                {
                    progress.UpdateProgress(ref processedCount, totalCount);
                    continue;
                }

                _problematicSprites.Add(importer);
                progress.UpdateProgress(ref processedCount, totalCount);
            }

            return _problematicSprites.Count > 0
                ? $"Found {_problematicSprites.Count} problematic sprites that need alpha channel fixes."
                : "No problematic sprites found. All sprites have proper alpha channels.";
        }

        private bool IsProblematicSprite(string path, out TextureImporter importer)
        {
            importer = null;

            if (AssetImporter.GetAtPath(path) is not TextureImporter
                {
                    textureType: TextureImporterType.Sprite
                } textureImporter)
                return false;

            importer = textureImporter;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (!texture)
                return false;

            var isNPOT = !texture.width.IsPowerOfTwo() && !texture.height.IsPowerOfTwo();
            var isRGB8 = !importer.DoesSourceTextureHaveAlpha();
            var hasCrunchEnabled = importer.crunchedCompression;

            return isNPOT && isRGB8 && hasCrunchEnabled;
        }

        private async UniTaskVoid FixAllSpritesAsync()
        {
            await EditorProgressTracker.CreateProgressAsync(
                "Fixing Sprites",
                "Processing problematic sprites...",
                ProcessAllSprites);
        }

        private async UniTask<string> ProcessAllSprites(IProgress<float> progress, CancellationToken cancellationToken)
        {
            var totalCount = _problematicSprites.Count;
            var processedCount = 0;
            var successCount = 0;
            var errorCount = 0;

            foreach (var importer in _problematicSprites)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await ProcessSpriteTexture(importer, progress, cancellationToken);
                if (result.Success)
                    successCount++;
                else
                    errorCount++;

                progress.UpdateProgress(ref processedCount, totalCount);
            }

            _problematicSprites.Clear();

            return $"Successfully fixed {successCount} sprites" +
                   (errorCount > 0 ? $" with {errorCount} errors." : ".");
        }

        private async UniTaskVoid AddAlphaPixelAsync(TextureImporter textureImporter)
        {
            if (!textureImporter)
                return;

            await EditorProgressTracker.CreateProgressAsync(
                "Processing Sprite",
                $"Adding alpha pixel to {Path.GetFileName(textureImporter.assetPath)}...",
                async (progress, cancellationToken) =>
                {
                    var result = await ProcessSpriteTexture(textureImporter, progress, cancellationToken);
                    return result.Success
                        ? "Alpha pixel added successfully"
                        : $"Failed to add alpha pixel: {result.Message}";
                });
        }

        private async UniTask<ResultData> ProcessSpriteTexture(
            TextureImporter textureImporter,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            if (!textureImporter)
                return new ResultData { Message = "No texture importer provided" };

            var path = textureImporter.assetPath;
            var originalSettings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(originalSettings);

            progress.Report(0.2f);

            try
            {
                var prepareResult = await PrepareTextureForEditing(textureImporter, path, cancellationToken);
                if (!prepareResult.Success)
                {
                    RestoreOriginalSettings(textureImporter, originalSettings, path);
                    return prepareResult;
                }

                progress.Report(0.4f);

                var modifyResult = await ModifyTextureAlpha(path, cancellationToken);
                if (!modifyResult.Success)
                {
                    RestoreOriginalSettings(textureImporter, originalSettings, path);
                    return modifyResult;
                }

                progress.Report(0.7f);

                var finalizeResult =
                    await FinalizeTextureSettings(textureImporter, originalSettings, path, cancellationToken);
                if (!finalizeResult.Success)
                {
                    RestoreOriginalSettings(textureImporter, originalSettings, path);
                    return finalizeResult;
                }

                progress.Report(1.0f);

                return new ResultData
                {
                    Success = true,
                    Message = "Alpha pixel added successfully"
                };
            }
            catch (Exception e)
            {
                RestoreOriginalSettings(textureImporter, originalSettings, path);
                return new ResultData { Message = e.Message };
            }
        }

        private void RestoreOriginalSettings(TextureImporter importer, TextureImporterSettings settings, string path)
        {
            importer.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private async UniTask<ResultData> PrepareTextureForEditing(
            TextureImporter importer, string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            importer.isReadable = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            await UniTask.Yield(cancellationToken);

            return new ResultData { Success = true };
        }

        private async UniTask<ResultData> ModifyTextureAlpha(
            string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (!texture)
                return new ResultData { Message = "Failed to load texture" };

            var newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            var pixels = texture.GetPixels();
            var lastPixel = texture.width * texture.height - 1;

            pixels[lastPixel] = new Color(
                pixels[lastPixel].r,
                pixels[lastPixel].g,
                pixels[lastPixel].b,
                0.99f
            );

            newTexture.SetPixels(pixels);
            newTexture.Apply();
            var pngData = newTexture.EncodeToPNG();

            await File.WriteAllBytesAsync(path, pngData, cancellationToken);

            DestroyImmediate(newTexture);

            await UniTask.Yield(cancellationToken);
            return new ResultData { Success = true };
        }

        private async UniTask<ResultData> FinalizeTextureSettings(
            TextureImporter importer, TextureImporterSettings originalSettings,
            string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.isReadable = originalSettings.readable;
            importer.textureType = TextureImporterType.Sprite;
            importer.sRGBTexture = true;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            await UniTask.Yield(cancellationToken);

            return new ResultData { Success = true };
        }
    }
}