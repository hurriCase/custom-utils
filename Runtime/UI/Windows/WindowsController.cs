using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.Windows.Registries;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using CustomUtils.Runtime.UI.Windows.Windows.Parameterized;
using CustomUtils.Runtime.UI.Windows.Windows.Plain;
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
        [SerializeField] private List<AssetReferenceT<GameObject>> _screenReferences;
        [SerializeField] private List<AssetReferenceT<GameObject>> _popupReferences;

        [SerializeField] private Transform _screensContainer;
        [SerializeField] private Transform _popupsContainer;

        public ReadOnlyReactiveProperty<Type> CurrentScreenType => _currentScreenType;
        public ReadOnlyReactiveProperty<Type> CurrentPopupType => _currentPopupType;

        private readonly ReactiveProperty<Type> _currentScreenType = new();
        private readonly ReactiveProperty<Type> _currentPopupType = new();

        private ScreenRegistry _screenRegistry;
        private PopupRegistry _popupRegistry;

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

            if (_popupReferences.Count > 0)
            {
                _popupRegistry = new PopupRegistry(
                    _currentPopupType,
                    _popupsContainer,
                    _objectResolver,
                    _addressablesLoader,
                    destroyCancellationToken);

                await _popupRegistry.LoadAsync(_popupReferences, sourceWithDestroy.Token);
            }

            if (_screenReferences.Count > 0)
            {
                _screenRegistry = new ScreenRegistry(
                    _currentScreenType,
                    _addressablesLoader,
                    _objectResolver,
                    _popupRegistry,
                    _screensContainer,
                    destroyCancellationToken);

                await _screenRegistry.LoadAsync(_screenReferences, sourceWithDestroy.Token);
            }
        }

        public async UniTask<SharedScreenBase> OpenScreen<TScreen>(CancellationToken token = default)
            where TScreen : ScreenBase
        {
            _popupRegistry.HideAll();
            return await _screenRegistry.Show<TScreen>(token);
        }

        public async UniTask<SharedScreenBase> OpenScreen<TParameterizedScreen, TParameters>(
            TParameters parameters,
            CancellationToken token = default)
            where TParameterizedScreen : ParameterizedScreenBase<TParameters>
        {
            _popupRegistry.HideAll();
            return await _screenRegistry.Show<TParameterizedScreen, TParameters>(parameters, token);
        }

        public async UniTask<SharedPopupBase> OpenPopup<TPopup>(CancellationToken token = default)
            where TPopup : PopupBase
            => await _popupRegistry.Show<TPopup>(token);

        public async UniTask ClosePopupAsync<TPopup>(CancellationToken token = default) where TPopup : SharedPopupBase
            => await _popupRegistry.CloseAsync<TPopup>(token);

        public async UniTask<SharedPopupBase> OpenPopup<TParameterizedPopup, TParameters>(
            TParameters parameters,
            CancellationToken token)
            where TParameterizedPopup : ParameterizedPopupBase<TParameters> =>
            await _popupRegistry.Show<TParameterizedPopup, TParameters>(parameters, token);

        public void BindScreenOpen<TScreen>(UIBehaviour component) where TScreen : ScreenBase
        {
            component.OnPointerClickAsObservable()
                .SubscribeAwait(
                    this,
                    static async (_, self, token) => await self.OpenScreen<TScreen>(token),
                    AwaitOperation.Drop)
                .RegisterTo(component.destroyCancellationToken);
        }

        public void BindPopupOpen<TPopup>(UIBehaviour component) where TPopup : PopupBase
        {
            component.OnPointerClickAsObservable()
                .SubscribeAwait(
                    this,
                    static async (_, self, token) => await self.OpenPopup<TPopup>(token),
                    AwaitOperation.Drop)
                .RegisterTo(component.destroyCancellationToken);
        }
    }
}