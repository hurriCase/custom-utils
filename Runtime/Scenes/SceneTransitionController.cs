using System;
using System.Threading;
using CustomUtils.Runtime.Scenes.Base;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using JetBrains.Annotations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace CustomUtils.Runtime.Scenes
{
    [PublicAPI]
    [Preserve]
    public sealed class SceneTransitionController : ISceneTransitionController
    {
        private const string TransitionSceneName = "Transition";

        private Scene _initialScene;
        private Scene _transitionScene;
        private SceneInstance _currentScene;

        private readonly ISceneLoader _sceneLoader;

        [Preserve]
        public SceneTransitionController(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
            _initialScene = SceneManager.GetActiveScene();
        }

        // Unity can't unload the last loaded scene, so an empty scene stays loaded and active between scenes.
        // Whatever is created mid-transition lands there instead of in a scene that's about to unload.
        public async UniTask UnloadCurrentSceneAsync()
        {
            if (!_transitionScene.IsValid())
                _transitionScene = SceneManager.CreateScene(TransitionSceneName);

            SceneManager.SetActiveScene(_transitionScene);

            if (_initialScene.isLoaded)
            {
                var unloadOperation = SceneManager.UnloadSceneAsync(_initialScene);
                await UniTask.WaitUntil(unloadOperation, static operation => operation.isDone);
            }

            await _sceneLoader.UnloadSceneAsync(_currentScene);
        }

        public async UniTask LoadSceneAsync(SceneReference scene, IProgress<float> progress, CancellationToken token)
        {
            _currentScene = await _sceneLoader.LoadSceneAsync(scene.Address, token, LoadSceneMode.Additive, progress);
            SceneManager.SetActiveScene(_currentScene.Scene);
        }
    }
}
