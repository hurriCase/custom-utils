using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using CustomUtils.Runtime.UI.Windows.Windows.Parameterized;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace CustomUtils.Runtime.UI.Windows.Registries
{
    internal abstract class WindowRegistry<TWindow> where TWindow : WindowBase
    {
        protected TWindow currentWindow;
        protected readonly CancellationToken lifetimeToken;

        private readonly Dictionary<Type, TWindow> _windows = new();
        private readonly Dictionary<Type, AssetReferenceT<GameObject>> _lazyReferences = new();
        private readonly Dictionary<Type, UniTask<TWindow>> _lazyLoads = new();
        private readonly Dictionary<TWindow, GameObject> _lazyPrefabs = new();

        private readonly ReactiveProperty<Type> _currentType;
        private readonly Transform _container;
        private readonly IObjectResolver _objectResolver;
        private readonly IAddressablesLoader _addressablesLoader;

        internal WindowRegistry(
            ReactiveProperty<Type> currentType,
            Transform container,
            IObjectResolver objectResolver,
            IAddressablesLoader addressablesLoader,
            CancellationToken lifetimeToken)
        {
            _currentType = currentType;
            _container = container;
            _objectResolver = objectResolver;
            _addressablesLoader = addressablesLoader;
            this.lifetimeToken = lifetimeToken;
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

        internal async UniTask<TWindow> Show<TConcreteWindow>(CancellationToken token) where TConcreteWindow : TWindow
        {
            var window = await GetAsync<TConcreteWindow>();
            if (!window)
                return null;

            _currentType.Value = typeof(TConcreteWindow);
            var openedWindow = await OpenWindow(window, token);
            return openedWindow;
        }

        internal async UniTask<TWindow> Show<TConcreteWindow, TParameters>(
            TParameters parameters,
            CancellationToken token)
            where TConcreteWindow : TWindow, IParameterizedWindow<TParameters>
        {
            var window = await GetAsync<TConcreteWindow>();
            if (!window)
                return null;

            ((IParameterizedWindow<TParameters>)window).SetParameters(parameters);
            _currentType.Value = typeof(TConcreteWindow);
            var openedWindow = await OpenWindow(window, token);
            return openedWindow;
        }

        internal async UniTask CloseAsync<TConcreteWindow>(CancellationToken token) where TConcreteWindow : TWindow
        {
            if (!_windows.TryGetValue(typeof(TConcreteWindow), out var window))
                return;

            await window.HideAsync(token);
        }

        protected abstract void OnRegistered(TWindow window);
        protected abstract UniTask<TWindow> OpenWindow(TWindow window, CancellationToken token);

        protected bool IsLazy(TWindow window) => _lazyPrefabs.ContainsKey(window);

        protected void Unload(TWindow window)
        {
            _windows.Remove(window.GetType());
            _lazyPrefabs.Remove(window, out var prefab);

            Object.Destroy(window.gameObject);
            _addressablesLoader.Release(prefab);
        }

        protected void SetCurrentType(Type type)
        {
            _currentType.Value = type;
        }

        private async UniTask<TWindow> GetAsync<TConcreteWindow>() where TConcreteWindow : TWindow
        {
            var windowType = typeof(TConcreteWindow);
            if (_windows.TryGetValue(windowType, out var window))
                return window;

            if (!_lazyReferences.TryGetValue(windowType, out var reference))
            {
                Debug.LogError($"[{nameof(WindowRegistry<TWindow>)}::{nameof(GetAsync)}]" +
                               $" No window registered for type: {windowType}");
                return null;
            }

            if (!_lazyLoads.TryGetValue(windowType, out var load))
            {
                load = LoadLazyAsync(reference).Preserve();
                _lazyLoads[windowType] = load;
            }

            try
            {
                return await load;
            }
            finally
            {
                _lazyLoads.Remove(windowType);
            }
        }

        private async UniTask<TWindow> LoadLazyAsync(AssetReference reference)
        {
            var prefab = await _addressablesLoader.LoadAsync<GameObject>(reference, lifetimeToken);
            var window = CreateWindow(prefab);
            if (window)
                _lazyPrefabs[window] = prefab;

            return window;
        }

        private TWindow CreateWindow(GameObject prefab)
        {
            var created = _objectResolver.Instantiate(prefab, _container);

            if (!created.TryGetComponent<TWindow>(out var window))
            {
                Debug.LogError($"[{nameof(WindowRegistry<TWindow>)}::{nameof(CreateWindow)}]" +
                               " Loaded GameObject does not contain " +
                               $"{typeof(TWindow).Name} component: {created.name}");
                return null;
            }

            _windows[window.GetType()] = window;
            window.BaseInitialize();
            window.Initialize();

            OnRegistered(window);
            return window;
        }
    }
}