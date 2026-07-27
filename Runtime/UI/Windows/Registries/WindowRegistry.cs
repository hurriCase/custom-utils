using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using CustomUtils.Runtime.UI.Windows.Windows.Parameterized;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace CustomUtils.Runtime.UI.Windows.Registries
{
    internal abstract class WindowRegistry<TWindow> where TWindow : WindowBase
    {
        protected TWindow currentWindow;

        private readonly Dictionary<Type, TWindow> _windows = new();

        private readonly ReactiveProperty<Type> _currentType;
        private readonly Transform _container;
        private readonly IObjectResolver _objectResolver;
        private readonly IAddressablesLoader _addressablesLoader;

        internal WindowRegistry(
            ReactiveProperty<Type> currentType,
            Transform container,
            IObjectResolver objectResolver,
            IAddressablesLoader addressablesLoader)
        {
            _currentType = currentType;
            _container = container;
            _objectResolver = objectResolver;
            _addressablesLoader = addressablesLoader;
        }

        internal async UniTask LoadAsync(List<AssetReferenceT<GameObject>> references, CancellationToken token)
        {
            foreach (var reference in references)
            {
                var loaded = await _addressablesLoader.LoadAsync<GameObject>(reference, token);
                var created = _objectResolver.Instantiate(loaded, _container);

                if (!created.TryGetComponent<TWindow>(out var window))
                {
                    Debug.LogError($"[{nameof(WindowRegistry<TWindow>)}::{nameof(LoadAsync)}]" +
                                   " Loaded GameObject does not contain " +
                                   $"{typeof(TWindow).Name} component: {created.name}");
                    continue;
                }

                _windows[window.GetType()] = window;
                window.BaseInitialize();
                window.Initialize();

                OnRegistered(window);
            }
        }

        internal async UniTask<TWindow> Show<TConcreteWindow>(CancellationToken token) where TConcreteWindow : TWindow
        {
            if (!TryGet<TConcreteWindow>(out var window))
                return null;

            var openedWindow = await OpenWindow(window, token);
            SetCurrentType(openedWindow.AsNullable()?.GetType());
            return openedWindow;
        }

        internal async UniTask<TWindow> Show<TConcreteWindow, TParameters>(
            TParameters parameters,
            CancellationToken token)
            where TConcreteWindow : TWindow, IParameterizedWindow<TParameters>
        {
            if (!TryGet<TConcreteWindow>(out var window))
                return null;

            ((IParameterizedWindow<TParameters>)window).SetParameters(parameters);
            var openedWindow = await OpenWindow(window, token);
            SetCurrentType(openedWindow.AsNullable()?.GetType());
            return openedWindow;
        }

        internal async UniTask CloseAsync<TConcreteWindow>(CancellationToken token) where TConcreteWindow : TWindow
        {
            if (!TryGet<TConcreteWindow>(out var window))
                return;

            await window.HideAsync(token);
        }

        protected abstract void OnRegistered(TWindow window);
        protected abstract UniTask<TWindow> OpenWindow(TWindow window, CancellationToken token);

        private bool TryGet<TConcreteWindow>(out TWindow window) where TConcreteWindow : TWindow
        {
            if (_windows.TryGetValue(typeof(TConcreteWindow), out window))
                return true;

            Debug.LogError($"[{nameof(WindowRegistry<TWindow>)}::{nameof(TryGet)}]" +
                           $" No window registered for type: {typeof(TConcreteWindow)}");
            return false;
        }

        protected void SetCurrentType(Type type)
        {
            _currentType.Value = type;
        }
    }
}