using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Scenes.Base
{
    /// <summary>
    /// Swaps the current scene for the next one. Whatever covers the swap is shown by the caller.
    /// </summary>
    [PublicAPI]
    public interface ISceneTransitionController
    {
        /// <summary>
        /// Unloads the initial scene and the current scene, if they are loaded.
        /// </summary>
        UniTask UnloadCurrentSceneAsync();

        /// <summary>
        /// Loads the destination scene additively and makes it the current scene.
        /// </summary>
        /// <param name="scene">Scene to load.</param>
        /// <param name="progress">Receives the load progress in the 0..1 range.</param>
        /// <param name="token">Cancellation token.</param>
        UniTask LoadSceneAsync(SceneReference scene, IProgress<float> progress, CancellationToken token);
    }
}
