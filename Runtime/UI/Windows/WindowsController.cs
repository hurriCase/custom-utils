using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using CustomUtils.Runtime.UI.Windows.Windows.Parameterized;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using VContainer;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    public class WindowsController : MonoBehaviour
    {
        [SerializeField] private List<AssetReferenceT<GameObject>> _windowReferences;
        [SerializeField] private List<LazyWindowReference> _lazyWindowReferences;

        public ReadOnlyReactiveProperty<Type> CurrentScreenType => _currentScreenType;
        public ReadOnlyReactiveProperty<Type> CurrentPopupType => _currentPopupType;

        private readonly ReactiveProperty<Type> _currentScreenType = new();
        private readonly ReactiveProperty<Type> _currentPopupType = new();

        private WindowRegistry _windowRegistry;

        private IObjectResolver _objectResolver;
        private IAddressablesLoader _addressablesLoader;

        [Inject]
        public void Inject(IObjectResolver objectResolver, IAddressablesLoader addressablesLoader)
        {
            _objectResolver = objectResolver;
            _addressablesLoader = addressablesLoader;
        }

        public virtual async UniTask InitializeAsync(CancellationToken token = default)
        {
            var sourceWithDestroy = token.CreateLinkedTokenSourceWithDestroy(this);

            _windowRegistry = new WindowRegistry(
                _currentScreenType,
                _currentPopupType,
                transform,
                _objectResolver,
                _addressablesLoader,
                destroyCancellationToken);

            _windowRegistry.RegisterLazy(_lazyWindowReferences);
            await _windowRegistry.LoadAsync(_windowReferences, sourceWithDestroy.Token);
        }

        public async UniTask<WindowBase> OpenScreen<TScreen>(CancellationToken token = default)
            where TScreen : WindowBase
        {
            var window = await _windowRegistry.GetAsync<TScreen>();
            return await _windowRegistry.OpenRootAsync(window, token);
        }

        public async UniTask<WindowBase> OpenScreen<TParameterizedScreen, TParameters>(
            TParameters parameters,
            CancellationToken token = default)
            where TParameterizedScreen : ParameterizedWindowBase<TParameters>
        {
            var window = await _windowRegistry.GetAsync<TParameterizedScreen, TParameters>(parameters);
            return await _windowRegistry.OpenRootAsync(window, token);
        }

        public async UniTask<WindowBase> OpenPopupClean<TPopup>(CancellationToken token = default)
            where TPopup : WindowBase
        {
            var window = await _windowRegistry.GetAsync<TPopup>();
            return await _windowRegistry.PushCleanAsync(window, token);
        }

        public async UniTask<WindowBase> OpenPopup<TPopup>(CancellationToken token = default)
            where TPopup : WindowBase
        {
            var window = await _windowRegistry.GetAsync<TPopup>();
            return await _windowRegistry.PushAsync(window, token);
        }

        public async UniTask<WindowBase> OpenPopup<TParameterizedPopup, TParameters>(
            TParameters parameters,
            CancellationToken token)
            where TParameterizedPopup : ParameterizedWindowBase<TParameters>
        {
            var window = await _windowRegistry.GetAsync<TParameterizedPopup, TParameters>(parameters);
            return await _windowRegistry.PushAsync(window, token);
        }

        public async UniTask ClosePopupAsync<TPopup>(CancellationToken token = default) where TPopup : WindowBase
        {
            await _windowRegistry.CloseAsync<TPopup>(token);
        }

        public void BindScreenOpen<TScreen>(UIBehaviour component) where TScreen : WindowBase
        {
            component.OnPointerClickAsObservable()
                .SubscribeAwait(
                    this,
                    static async (_, self, token) => await self.OpenScreen<TScreen>(token),
                    AwaitOperation.Drop)
                .RegisterTo(component.destroyCancellationToken);
        }

        public void BindPopupOpen<TPopup>(UIBehaviour component) where TPopup : WindowBase
        {
            component.OnPointerClickAsObservable()
                .SubscribeAwait(
                    this,
                    static async (_, self, token) => await self.OpenPopup<TPopup>(token),
                    AwaitOperation.Drop)
                .RegisterTo(component.destroyCancellationToken);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var reference in _lazyWindowReferences)
                reference.UpdateWindowType();
        }
#endif
    }
}
