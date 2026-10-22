using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace CustomUtils.Runtime.Scenes.Base
{
    /// <summary>
    /// Interface for loading and unloading Addressable scenes.
    /// </summary>
    [PublicAPI]
    public interface ISceneLoader
    {
        /// <summary>
        /// Loads an Addressable scene asynchronously.
        /// </summary>
        /// <param name="sceneAddress">Addressable scene address to load.</param>
        /// <param name="token">Cancellation token.</param>
        /// <param name="loadMode">Scene loading mode. Defaults to Single.</param>
        /// <param name="progress">Receives the load progress (download and scene load) in the 0..1 range.</param>
        UniTask<SceneInstance> LoadSceneAsync(
            string sceneAddress,
            CancellationToken token = default,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            IProgress<float> progress = null);

        /// <summary>
        /// Unloads the specified scene instance if it is valid.
        /// </summary>
        /// <param name="sceneInstance">Scene instance to unload.</param>
        UniTask UnloadSceneAsync(SceneInstance sceneInstance);
    }
}