using System;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Scenes.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace CustomUtils.Runtime.Scenes
{
    [PublicAPI]
    [Preserve]
    public sealed class SceneLoader : ISceneLoader
    {
        private SceneInstance _sceneInstance;

        public async UniTask<SceneInstance> LoadSceneAsync(
            string sceneAddress,
            CancellationToken token,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            IProgress<float> progress = null)
        {
            try
            {
#if ADDRESSABLES_LOG_ALL
                using var stopWatchScope = AddressablesLogger.LogWithTimePast("[SceneLoader::LoadSceneAsync]");
#endif

                AddressablesLogger.Log($"[SceneLoader::LoadSceneAsync] Start loading scene: {sceneAddress}");

                var loadHandle = Addressables.LoadSceneAsync(sceneAddress, loadMode);

                while (!loadHandle.IsDone)
                {
                    progress?.Report(loadHandle.PercentComplete);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                var currentScene = await loadHandle.WithCancellation(token);
                progress?.Report(1f);

                AddressablesLogger.Log($"[SceneLoader::LoadSceneAsync] Scene loaded successfully: {sceneAddress}");

                return currentScene;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }

        public async UniTask UnloadSceneAsync(SceneInstance sceneInstance)
        {
            if (!sceneInstance.Scene.IsValid())
                return;

            AddressablesLogger.Log($"[SceneLoader::UnloadSceneAsync] Start unloading scene: {sceneInstance.Scene.name}");

            await Addressables.UnloadSceneAsync(sceneInstance).Task;

            AddressablesLogger.Log($"[SceneLoader::UnloadSceneAsync] Scene unloaded: {sceneInstance.Scene.name}");
        }
    }
}