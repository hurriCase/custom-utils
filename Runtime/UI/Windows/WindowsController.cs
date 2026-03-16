using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Windows.Registries;
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
        public Type CurrentPopupType => _popupRegistry.CurrentPopupType;

        private readonly ReactiveProperty<Type> _currentScreenType = new();

        private ScreenRegistry _screenRegistry;
        private PopupRegistry _popupRegistry;

        private IObjectResolver _objectResolver;
        private IAddressablesLoader _addressablesLoader;

        [Inject]
        public void Inject(IObjectResolver objectResolver, IAddressablesLoader addressablesLoader)
        {
            _objectResolver = objectResolver;
            _addressablesLoader = addressablesLoader;

            _screenRegistry = new ScreenRegistry(
                _currentScreenType,
                _screensContainer,
                objectResolver,
                addressablesLoader);

            _popupRegistry = new PopupRegistry(
                _popupsContainer,
                objectResolver,
                addressablesLoader,
                destroyCancellationToken);
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            var sourceWithDestroy = cancellationToken.CreateLinkedTokenSourceWithDestroy(this);

            await _screenRegistry.LoadAsync(_screenReferences, sourceWithDestroy.Token);
            await _popupRegistry.LoadAsync(_popupReferences, sourceWithDestroy.Token);
        }

        public void OpenScreen<TScreen>() where TScreen : ScreenBase
        {
            _popupRegistry.HideAll();
            _screenRegistry.Open<TScreen>();
        }

        public void OpenScreen<TParameterizedScreen, TParameters>(TParameters parameters)
            where TParameterizedScreen : ParameterizedScreenBase<TParameters>
        {
            _popupRegistry.HideAll();
            _screenRegistry.Open<TParameterizedScreen, TParameters>(parameters);
        }

        public void OpenPopup<TPopup>() where TPopup : PopupBase
        {
            _popupRegistry.Open<TPopup>();
        }

        public void OpenPopup<TParameterizedPopup, TParameters>(TParameters parameters)
            where TParameterizedPopup : ParameterizedPopupBase<TParameters>
        {
            _popupRegistry.Open<TParameterizedPopup, TParameters>(parameters);
        }

        public void BindScreenOpen<TScreen>(UIBehaviour component) where TScreen : ScreenBase
        {
            component.OnPointerClickAsObservable()
                .Subscribe(this, static (_, self) => self.OpenScreen<TScreen>())
                .RegisterTo(component.destroyCancellationToken);
        }

        public void BindPopupOpen<TPopup>(UIBehaviour component) where TPopup : PopupBase
        {
            component.OnPointerClickAsObservable()
                .Subscribe(this, static (_, self) => self.OpenPopup<TPopup>())
                .RegisterTo(component.destroyCancellationToken);
        }
    }
}