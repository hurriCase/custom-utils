using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomUtils.Editor.Scripts
{
    internal sealed class FlatScriptsCreatorWindow : WindowBase
    {
        private Object _sourceFolder;
        private Object _targetFolder;

        [MenuItem(MenuItemNames.FlatScriptMenuName)]
        internal static void ShowWindow()
        {
            GetWindow<FlatScriptsCreatorWindow>(nameof(FlatScriptsCreatorWindow).ToSpacedWords());
        }

        protected override void DrawWindowContent()
        {
            DrawSection("Folder Selection", DrawFolderSelectionContent);

            if (!_sourceFolder || !_targetFolder)
                EditorVisualControls.WarningBox("You need to assign folder before creating flat scripts");

            GUI.enabled = _sourceFolder && _targetFolder;

            if (EditorVisualControls.Button("Create Flat Scripts", GUILayout.Height(30)))
                CreateFlatScripts().Forget();

            GUI.enabled = true;
        }

        private void DrawFolderSelectionContent()
        {
            _sourceFolder = EditorStateControls.ObjectField("Source Folder:", _sourceFolder, typeof(DefaultAsset));
            _targetFolder = EditorStateControls.ObjectField("Target Folder:", _targetFolder, typeof(DefaultAsset));
        }

        private async UniTaskVoid CreateFlatScripts()
        {
            var sourcePath = AssetDatabase.GetAssetPath(_sourceFolder);
            var targetPath = AssetDatabase.GetAssetPath(_targetFolder);

            await EditorProgressTracker.CreateProgressAsync("Creating Flat Scripts", "Scanning for scripts...",
                async (progress, cancellationToken)
                    => await ProcessFilesWithProgress(sourcePath, targetPath, progress, cancellationToken));
        }

        private async UniTask<string> ProcessFilesWithProgress(string sourcePath, string targetPath,
            IProgress<float> progress, CancellationToken cancellationToken)
        {
            var csFiles = await UniTask.RunOnThreadPool(
                () => Directory.GetFiles(sourcePath, "*.cs", SearchOption.AllDirectories),
                cancellationToken: cancellationToken);

            progress.Report(0.1f);

            var copiedFiles = await CopyFilesWithProgressAsync(csFiles, targetPath, progress, cancellationToken);

            await UniTask.SwitchToMainThread(cancellationToken);

            AssetDatabase.Refresh();

            return $"Successfully copied {copiedFiles} scripts to flat folder structure.";
        }

        private async UniTask<int> CopyFilesWithProgressAsync(IReadOnlyCollection<string> files, string targetPath,
            IProgress<float> progress, CancellationToken token)
        {
            var totalFiles = files.Count;
            var copiedFiles = 0;

            foreach (var file in files)
            {
                token.ThrowIfCancellationRequested();

                var targetFilePath = GetUniqueTargetPath(file, targetPath);

                await UniTask.RunOnThreadPool(() => File.Copy(file, targetFilePath), cancellationToken: token);

                copiedFiles++;
                var progressValue = 0.1f + 0.8f * copiedFiles / totalFiles;
                progress.Report(progressValue);
            }

            return copiedFiles;
        }

        private string GetUniqueTargetPath(string sourceFile, string targetPath)
        {
            var fileName = Path.GetFileName(sourceFile);
            var targetFilePath = Path.Combine(targetPath, fileName);

            if (!File.Exists(targetFilePath))
                return targetFilePath;

            var uniqueName =
                $"{Path.GetFileNameWithoutExtension(sourceFile)}_{Guid.NewGuid()}{Path.GetExtension(sourceFile)}";

            targetFilePath = Path.Combine(targetPath, uniqueName);

            return targetFilePath;
        }
    }
}