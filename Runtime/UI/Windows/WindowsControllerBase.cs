using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.CustomTypes.Collections;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    public abstract class WindowsControllerBase<TScreenEnum, TPopUpEnum> : MonoBehaviour
        where TScreenEnum : unmanaged, Enum
        where TPopUpEnum : unmanaged, Enum
    {
        [SerializeField] private EnumArray<TScreenEnum, AssetReferenceT<GameObject>> _screenReferences;
        [SerializeField] private EnumArray<TPopUpEnum, AssetReferenceT<GameObject>> _popUpReferences;

        [SerializeField] private Transform _screensContainer;
        [SerializeField] private Transform _popUpsContainer;

        public ReadOnlyReactiveProperty<TScreenEnum> CurrentScreenType => _currentScreenType;
        private readonly ReactiveProperty<TScreenEnum> _currentScreenType = new();

        public TPopUpEnum CurrentPopUpType { get; private set; }

        private readonly EnumArray<TScreenEnum, ScreenBase> _createdScreens = new();
        private readonly EnumArray<TPopUpEnum, PopUpBase> _createdPopUps = new();
        private readonly Stack<PopUpBase> _previousOpenedPopUps = new();

        private PopUpBase _currentOpenedPopUp;
        private ScreenBase _currentScreen;

        private IObjectResolver _objectResolver;
        private IAddressablesLoader _addressablesLoader;

        [Inject]
        public void Inject(IObjectResolver objectResolver, IAddressablesLoader addressablesLoader)
        {
            _objectResolver = objectResolver;
            _addressablesLoader = addressablesLoader;
        }

        public async UniTask InitAsync(CancellationToken cancellationToken)
        {
            var sourceWithDestroy = cancellationToken.CreateLinkedTokenSourceWithDestroy(this);

            await InitScreensAsync(sourceWithDestroy.Token);
            await InitPopUpAsync(sourceWithDestroy.Token);
        }

        private async UniTask InitPopUpAsync(CancellationToken cancellationToken)
        {
            foreach (var (popUpType, popUpReference) in _popUpReferences.AsTuples())
            {
                var loadedPopUp = await _addressablesLoader.LoadAsync<GameObject>(popUpReference, cancellationToken);
                var createdWindow = _objectResolver.Instantiate(loadedPopUp, _popUpsContainer);

                if (createdWindow.TryGetComponent<PopUpBase>(out var popUpBase) is false)
                    continue;

                _createdPopUps[popUpType] = popUpBase;

                popUpBase.BaseInitialize();
                popUpBase.Initialize();
                popUpBase.HideImmediately();
                popUpBase.OnPopUpHidden.SubscribeUntilDestroy(this, static self => self.HandlePopUpHide());
            }
        }

        public void OpenScreenByType(TScreenEnum screenType)
        {
            HideAllPopUps();

            var screenBase = _createdScreens[screenType];

            if (!screenBase)
            {
                var message = ZString.Format("[WindowsControllerBase::OpenScreenByType] " +
                                             "There is no screen with type: {0}", screenType);
                Debug.LogError(message);
                return;
            }

            if (_currentScreen)
                _currentScreen.HideAsync();

            _currentScreen = screenBase;
            _currentScreenType.Value = screenType;
            screenBase.ShowAsync();
        }

        public void OpenPopUpByType(TPopUpEnum popUpType)
        {
            var popUpBase = _createdPopUps[popUpType];

            if (!popUpBase)
            {
                var message = ZString.Format("[WindowsControllerBase::OpenPopUpByType] There is no pop up with type: {0}",
                    popUpType);
                Debug.LogError(message);
                return;
            }

            OpenPopUpAsync(popUpBase, popUpType).Forget();
        }

        public TPopUpType OpenPopUp<TPopUpType>() where TPopUpType : PopUpBase
        {
            foreach (var (popUpEnum, popUpBase) in _createdPopUps.AsTuples())
            {
                if (popUpBase.GetType() != typeof(TPopUpType))
                    continue;

                OpenPopUpAsync(popUpBase, popUpEnum).Forget();
                return popUpBase as TPopUpType;
            }

            Debug.LogError(ZString.Format("[WindowsControllerBase::OpenPopUp] There is no pop up with type: {0}",
                typeof(TPopUpType)));

            return null;
        }

        public void BindPopUpOpen(UIBehaviour component, TPopUpEnum popUpType)
        {
            component.OnPointerClickAsObservable()
                .Subscribe((self: this, popUpType), static (_, tuple) => tuple.self.OpenPopUpByType(tuple.popUpType))
                .RegisterTo(component.destroyCancellationToken);
        }

        public void BindScreenOpen(UIBehaviour component, TScreenEnum screenType)
        {
            component.OnPointerClickAsObservable()
                .Subscribe((self: this, screenType), static (_, tuple) => tuple.self.OpenScreenByType(tuple.screenType))
                .RegisterTo(component.destroyCancellationToken);
        }

        private async UniTask InitScreensAsync(CancellationToken cancellationToken)
        {
            foreach (var (screenType, screenReference) in _screenReferences.AsTuples())
            {
                var loadedScreen = await _addressablesLoader.LoadAsync<GameObject>(screenReference, cancellationToken);
                var createdWindow = _objectResolver.Instantiate(loadedScreen, _screensContainer);

                if (createdWindow.TryGetComponent<ScreenBase>(out var screenBase) is false)
                    continue;

                _createdScreens[screenType] = screenBase;

                screenBase.BaseInitialize();
                screenBase.Initialize();

                if (screenBase.InitialWindow)
                {
                    _currentScreenType.Value = screenType;
                    continue;
                }

                screenBase.HideImmediately();
            }
        }

        private async UniTask OpenPopUpAsync(PopUpBase popUpBase, TPopUpEnum popUpEnum)
        {
            if (_currentOpenedPopUp && popUpBase.IsInFrontOf(_currentOpenedPopUp) is false)
                popUpBase.transform.SetAsLastSibling();

            await popUpBase.ShowAsync();

            if (_currentOpenedPopUp)
            {
                _previousOpenedPopUps.Push(_currentOpenedPopUp);

                if (popUpBase.IsSingle)
                    _currentOpenedPopUp.HideImmediately();
            }

            _currentOpenedPopUp = popUpBase;
            CurrentPopUpType = popUpEnum;
        }

        private void HideAllPopUps()
        {
            CurrentPopUpType = default;

            if (_currentOpenedPopUp)
            {
                _currentOpenedPopUp.HideImmediately();
                _currentOpenedPopUp = null;
            }

            _previousOpenedPopUps.Clear();
        }

        private void HandlePopUpHide()
        {
            CurrentPopUpType = default;

            var needShow = _currentOpenedPopUp && _currentOpenedPopUp.IsSingle;
            _currentOpenedPopUp = null;

            if (_previousOpenedPopUps.TryPop(out var previousPopUp) is false)
                return;

            _currentOpenedPopUp = previousPopUp;
            if (needShow)
                previousPopUp.ShowAsync().Forget();
        }
    }
}