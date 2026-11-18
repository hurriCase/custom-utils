using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace CustomUtils.Runtime.UI.Windows
{
    internal sealed class WindowLoader : IDisposable
    {
        internal Observable<WindowBase> OnCreated => _created;

        private readonly Subject<WindowBase> _created = new();
        private readonly Dictionary<Type, UniTask<WindowBase>> _windows = new();
        private readonly Dictionary<Type, AssetReferenceT<GameObject>> _lazyReferences = new();
        private readonly Dictionary<WindowBase, GameObject> _lazyPrefabs = new();

        private readonly IAddressablesLoader _addressablesLoader;
        private readonly IObjectResolver _objectResolver;
        private readonly CancellationToken _lifetimeToken;
        private readonly Transform _parent;

        internal WindowLoader(
            IAddressablesLoader addressablesLoader,
            IObjectResolver objectResolver,
            CancellationToken lifetimeToken,
            Transform parent)
        {
            _addressablesLoader = addressablesLoader;
            _objectResolver = objectResolver;
            _lifetimeToken = lifetimeToken;
            _parent = parent;
        }

        internal async UniTask LoadAsync(
            IReadOnlyList<AssetReferenceT<GameObject>> windowReferences,
            IReadOnlyList<LazyWindowReference> lazyWindowReferences,
            CancellationToken token)
        {
            foreach (var lazyReference in lazyWindowReferences)
                _lazyReferences[lazyReference.WindowType] = lazyReference.Prefab;

            foreach (var reference in windowReferences)
            {
                var prefab = await _addressablesLoader.LoadAsync<GameObject>(reference, token);
                var window = CreateWindow(prefab);
                _windows[window.GetType()] = UniTask.FromResult(window);
            }
        }

        internal async UniTask<TWindow> GetAsync<TWindow>() where TWindow : WindowBase
        {
            var windowType = typeof(TWindow);
            if (!_windows.TryGetValue(windowType, out var load))
            {
                load = LoadLazyAsync(windowType).Preserve();
                _windows[windowType] = load;
            }

            try
            {
                var window = await load;
                return (TWindow)window;
            }
            finally
            {
                if (!load.Status.IsCompletedSuccessfully())
                    _windows.Remove(windowType);
            }
        }

        internal bool TryGetLoad(Type windowType, out UniTask<WindowBase> load)
            => _windows.TryGetValue(windowType, out load);

        public void Dispose()
        {
            _created.Dispose();
        }

        private async UniTask<WindowBase> LoadLazyAsync(Type windowType)
        {
            var reference = _lazyReferences[windowType];
            var prefab = await _addressablesLoader.LoadAsync<GameObject>(reference, _lifetimeToken);
            var window = CreateWindow(prefab);
            _lazyPrefabs[window] = prefab;

            window.OnHidden
                .SubscribeSelf(this, static (hiddenWindow, self) => self.UnloadWhenUnusedAsync(hiddenWindow).Forget())
                .RegisterTo(window.destroyCancellationToken);

            return window;
        }

        private async UniTaskVoid UnloadWhenUnusedAsync(WindowBase window)
        {
            await UniTask.NextFrame(_lifetimeToken);

            var isUsed = !window || window.State != WindowState.Closed;
            if (isUsed)
                return;

            _windows.Remove(window.GetType());
            _lazyPrefabs.Remove(window, out var prefab);

            Object.Destroy(window.gameObject);
            _addressablesLoader.Release(prefab);
        }

        private WindowBase CreateWindow(GameObject prefab)
        {
            var created = _objectResolver.Instantiate(prefab, _parent);
            var window = created.GetComponent<WindowBase>();
            window.BaseInitialize();
            window.Initialize();
            _created.OnNext(window);
            return window;
        }
    }
}
