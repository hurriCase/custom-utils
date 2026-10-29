using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using CustomUtils.Runtime.UI.Windows.Windows.Parameterized;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace CustomUtils.Runtime.UI.Windows
{
    internal sealed class WindowRegistry
    {
        private WindowBase TopWindow => _stackedWindows.Count > 0 ? _stackedWindows[^1] : _rootWindow;

        private readonly Dictionary<Type, WindowBase> _windows = new();
        private readonly List<WindowBase> _stackedWindows = new();

        private readonly Dictionary<Type, AssetReferenceT<GameObject>> _lazyReferences = new();
        private readonly Dictionary<Type, UniTask<WindowBase>> _lazyLoads = new();
        private readonly Dictionary<WindowBase, GameObject> _lazyPrefabs = new();

        private readonly ReactiveProperty<Type> _currentScreenType;
        private readonly ReactiveProperty<Type> _currentPopupType;
        private readonly Transform _container;
        private readonly IObjectResolver _objectResolver;
        private readonly IAddressablesLoader _addressablesLoader;
        private readonly CancellationToken _token;

        private WindowBase _rootWindow;

        internal WindowRegistry(
            ReactiveProperty<Type> currentScreenType,
            ReactiveProperty<Type> currentPopupType,
            Transform container,
            IObjectResolver objectResolver,
            IAddressablesLoader addressablesLoader,
            CancellationToken token)
        {
            _currentScreenType = currentScreenType;
            _currentPopupType = currentPopupType;
            _container = container;
            _objectResolver = objectResolver;
            _addressablesLoader = addressablesLoader;
            _token = token;
        }

        internal async UniTask LoadAsync(List<AssetReferenceT<GameObject>> references, CancellationToken token)
        {
            foreach (var reference in references)
            {
                var loaded = await _addressablesLoader.LoadAsync<GameObject>(reference, token);
                CreateWindow(loaded);
            }
        }

        internal void RegisterLazy(List<LazyWindowReference> references)
        {
            foreach (var reference in references)
                _lazyReferences[reference.WindowType] = reference.Prefab;
        }

        internal async UniTask<TWindow> GetAsync<TWindow>() where TWindow : WindowBase
        {
            var windowType = typeof(TWindow);
            if (_windows.TryGetValue(windowType, out var window))
                return (TWindow)window;

            if (!_lazyLoads.TryGetValue(windowType, out var load))
            {
                load = LoadLazyAsync(_lazyReferences[windowType]).Preserve();
                _lazyLoads[windowType] = load;
            }

            try
            {
                var loadedWindow = await load;
                return (TWindow)loadedWindow;
            }
            finally
            {
                _lazyLoads.Remove(windowType);
            }
        }

        internal async UniTask<TWindow> GetAsync<TWindow, TParameters>(TParameters parameters)
            where TWindow : ParameterizedWindowBase<TParameters>
        {
            var window = await GetAsync<TWindow>();
            window.SetParameters(parameters);
            return window;
        }

        internal async UniTask CloseAsync<TWindow>(CancellationToken token) where TWindow : WindowBase
        {
            if (!_windows.TryGetValue(typeof(TWindow), out var window))
                return;

            await window.HideAsync(token);
        }

        internal async UniTask<WindowBase> OpenRootAsync(WindowBase window, CancellationToken token)
        {
            _currentPopupType.Value = null;
            _currentScreenType.Value = window.GetType();

            var stackedWindows = _stackedWindows.ToArray();
            _stackedWindows.Clear();

            var previousRootWindow = _rootWindow;
            _rootWindow = window;

            var transitions = new List<UniTask>();
            foreach (var stackedWindow in stackedWindows)
            {
                var hideTask = stackedWindow.HideAsync(token);
                transitions.Add(hideTask);
            }

            if (previousRootWindow && previousRootWindow != window)
            {
                var hideTask = previousRootWindow.HideAsync(token);
                transitions.Add(hideTask);
            }

            window.transform.SetAsLastSibling();
            var showTask = ShowOnOwnLifetimeAsync(window, token);
            transitions.Add(showTask);

            await UniTask.WhenAll(transitions);

            window.transform.SetAsFirstSibling();
            return window;
        }

        internal async UniTask<WindowBase> PushAsync(WindowBase window, CancellationToken token)
        {
            _stackedWindows.Remove(window);

            var coveredWindow = TopWindow;
            if (coveredWindow && !window.IsInFrontOf(coveredWindow))
                window.transform.SetAsLastSibling();

            _stackedWindows.Add(window);
            _currentPopupType.Value = window.GetType();

            var transitions = new List<UniTask>();
            CollectTransitions(transitions, window, token);
            await UniTask.WhenAll(transitions);
            return window;
        }

        internal async UniTask<WindowBase> PushCleanAsync(WindowBase window, CancellationToken token)
        {
            var stackedWindows = _stackedWindows.ToArray();
            _stackedWindows.Clear();

            foreach (var stackedWindow in stackedWindows)
                stackedWindow.HideImmediately();

            return await PushAsync(window, token);
        }

        private async UniTask<WindowBase> LoadLazyAsync(AssetReference reference)
        {
            var prefab = await _addressablesLoader.LoadAsync<GameObject>(reference, _token);
            var window = CreateWindow(prefab);
            if (window)
                _lazyPrefabs[window] = prefab;

            return window;
        }

        private WindowBase CreateWindow(GameObject prefab)
        {
            var created = _objectResolver.Instantiate(prefab, _container);

            if (!created.TryGetComponent<WindowBase>(out var window))
            {
                Debug.LogError($"[{nameof(WindowRegistry)}::{nameof(CreateWindow)}]" +
                               " Loaded GameObject does not contain " +
                               $"{nameof(WindowBase)} component: {created.name}");
                return null;
            }

            _windows[window.GetType()] = window;
            window.BaseInitialize();
            window.Initialize();

            Register(window);
            return window;
        }

        private void Register(WindowBase window)
        {
            window.OnHiding
                .SubscribeSelf(this, static (hidingWindow, self) => self.HandleHiding(hidingWindow))
                .RegisterTo(_token);

            window.OnHidden
                .SubscribeSelf(this, static (hiddenWindow, self) => self.UnloadWhenUnusedAsync(hiddenWindow).Forget())
                .RegisterTo(_token);

            if (!window.InitialWindow)
            {
                window.HideImmediately(isSilent: true);
                return;
            }

            _rootWindow = window;
            _currentScreenType.Value = window.GetType();
            window.transform.SetAsFirstSibling();
            window.ShowAsync(_token).Forget();
        }

        private void HandleHiding(WindowBase window)
        {
            var index = _stackedWindows.IndexOf(window);
            if (index < 0)
                return;

            var isTopWindow = index == _stackedWindows.Count - 1;
            _stackedWindows.RemoveAt(index);
            _currentPopupType.Value = _stackedWindows.Count > 0 ? _stackedWindows[^1].GetType() : null;

            var isRootRevealed = isTopWindow && _stackedWindows.Count == 0;
            var focusedWindow = isRootRevealed ? _rootWindow : null;

            RevealWhenUncoveredAsync(window, focusedWindow).Forget();
        }

        // The windows below appear at once, but play their show animation only when the hiding window
        // is gone, otherwise the animation plays behind it.
        private async UniTaskVoid RevealWhenUncoveredAsync(WindowBase hidingWindow, Object focusedWindow)
        {
            UncoverWindows();

            await hidingWindow.OnHidden.FirstAsync(_token);

            var transitions = new List<UniTask>();
            CollectTransitions(transitions, focusedWindow, _token);
            await UniTask.WhenAll(transitions);
        }

        private void UncoverWindows()
        {
            for (var i = _stackedWindows.Count - 1; i >= 0; i--)
            {
                var stackedWindow = _stackedWindows[i];
                stackedWindow.Uncover();
                if (stackedWindow.Mode == WindowMode.Fullscreen)
                    return;
            }

            if (_rootWindow)
                _rootWindow.Uncover();
        }

        private void CollectTransitions(List<UniTask> transitions, Object focusedWindow, CancellationToken token)
        {
            var isCovered = false;
            for (var i = _stackedWindows.Count - 1; i >= 0; i--)
            {
                var stackedWindow = _stackedWindows[i];
                AddTransition(transitions, stackedWindow, focusedWindow, isCovered, token);
                isCovered |= stackedWindow.Mode == WindowMode.Fullscreen;
            }

            if (_rootWindow)
                AddTransition(transitions, _rootWindow, focusedWindow, isCovered, token);
        }

        private void AddTransition(
            List<UniTask> transitions,
            WindowBase window,
            Object focusedWindow,
            bool isCovered,
            CancellationToken token)
        {
            if (isCovered)
            {
                if (!window.IsVisible)
                    return;

                var coverTask = window.PlayHideAnimationAsync(token);
                transitions.Add(coverTask);

                return;
            }

            if (window != focusedWindow && window.IsVisible)
                return;

            var showTask = ShowOnOwnLifetimeAsync(window, token);
            transitions.Add(showTask);
        }

        private UniTask ShowOnOwnLifetimeAsync(WindowBase window, CancellationToken token)
            => window.ShowAsync(window.destroyCancellationToken).AttachExternalCancellation(token);

        private async UniTaskVoid UnloadWhenUnusedAsync(WindowBase window)
        {
            if (!_lazyPrefabs.ContainsKey(window))
                return;

            await UniTask.NextFrame(_token);

            var isUsed = !window || window.IsVisible || window == _rootWindow || _stackedWindows.Contains(window);
            if (isUsed)
                return;

            _windows.Remove(window.GetType());
            _lazyPrefabs.Remove(window, out var prefab);

            Object.Destroy(window.gameObject);
            _addressablesLoader.Release(prefab);
        }
    }
}
