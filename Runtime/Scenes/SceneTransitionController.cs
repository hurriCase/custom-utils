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
        public bool IsLoading { get; private set; }

        private Scene _initialScene;
        private SceneInstance _transitionScene;
        private SceneInstance _currentScene;

        private readonly ISceneLoader _sceneLoader;

        [Preserve]
        public SceneTransitionController(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
            _initialScene = SceneManager.GetActiveScene();
        }

        public async UniTask ShowTransitionSceneAsync(SceneReference transitionScene, CancellationToken token)
        {
            IsLoading = true;

            _transitionScene = await _sceneLoader.LoadSceneAsync(
                transitionScene.Address,
                token,
                LoadSceneMode.Additive);
        }

        public async UniTask CompleteTransitionAsync(
            SceneReference destinationScene,
            CancellationToken token,
            bool isEndAfterTransition = true)
        {
            if (_initialScene.isLoaded)
                SceneManager.UnloadSceneAsync(_initialScene);

            if (_currentScene.Scene.IsValid())
                _sceneLoader.TryUnloadScene(_currentScene);

            _currentScene = await _sceneLoader.LoadSceneAsync(destinationScene.Address, token, LoadSceneMode.Additive);

            IsLoading = false;

            if (isEndAfterTransition)
                EndTransition().Forget();
        }

        public async UniTask StartTransition(
            SceneReference transitionScene,
            SceneReference destinationScene,
            CancellationToken token,
            bool isEndAfterTransition = true)
        {
            IsLoading = true;

            _transitionScene = await _sceneLoader.LoadSceneAsync(
                transitionScene.Address,
                token,
                LoadSceneMode.Additive);

            if (_initialScene.isLoaded)
                SceneManager.UnloadSceneAsync(_initialScene);

            if (_currentScene.Scene.IsValid())
                _sceneLoader.TryUnloadScene(_currentScene);

            _currentScene = await _sceneLoader.LoadSceneAsync(destinationScene.Address, token, LoadSceneMode.Additive);

            IsLoading = false;

            if (isEndAfterTransition)
                EndTransition().Forget();
        }

        public async UniTask EndTransition()
        {
            await UniTask.WaitUntil(this, static self => !self.IsLoading);

            _sceneLoader.TryUnloadScene(_transitionScene);
        }
    }
}
