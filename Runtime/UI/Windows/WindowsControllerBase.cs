using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.CustomTypes.Collections;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.Formatter;
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
    public abstract class WindowsControllerBase<TScreenEnum, TPopupEnum> : MonoBehaviour
        where TScreenEnum : unmanaged, Enum
        where TPopupEnum : unmanaged, Enum
    {
        [SerializeField] private EnumArray<TScreenEnum, AssetReferenceT<GameObject>> _screenReferences;
        [SerializeField] private EnumArray<TPopupEnum, AssetReferenceT<GameObject>> _popupReferences;

        [SerializeField] private Transform _screensContainer;
        [SerializeField] private Transform _popupsContainer;

        public ReadOnlyReactiveProperty<TScreenEnum> CurrentScreenType => _currentScreenType;
        private readonly ReactiveProperty<TScreenEnum> _currentScreenType = new();

        public TPopupEnum CurrentPopupType { get; private set; }

        private readonly EnumArray<TScreenEnum, ScreenBase> _createdScreens = new();
        private readonly EnumArray<TPopupEnum, PopupBase> _createdPopups = new();
        private readonly Stack<PopupBase> _previousOpenedPopups = new();

        private PopupBase _currentOpenedPopup;
        private ScreenBase _currentScreen;

        private IObjectResolver _objectResolver;
        private IAddressablesLoader _addressablesLoader;

        [Inject]
        public void Inject(IObjectResolver objectResolver, IAddressablesLoader addressablesLoader)
        {
            _objectResolver = objectResolver;
            _addressablesLoader = addressablesLoader;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            var sourceWithDestroy = cancellationToken.CreateLinkedTokenSourceWithDestroy(this);

            await InitializeScreensAsync(sourceWithDestroy.Token);
            await InitializePopupsAsync(sourceWithDestroy.Token);
        }

        private async UniTask InitializeScreensAsync(CancellationToken cancellationToken)
        {
            foreach (var (screenType, screenReference) in _screenReferences.AsTuples())
            {
                var loadedScreen = await _addressablesLoader.LoadAsync<GameObject>(screenReference, cancellationToken);
                var createdWindow = _objectResolver.Instantiate(loadedScreen, _screensContainer);

                if (!createdWindow.TryGetComponent<ScreenBase>(out var screenBase))
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

        private async UniTask InitializePopupsAsync(CancellationToken cancellationToken)
        {
            foreach (var (popupType, popupReference) in _popupReferences.AsTuples())
            {
                var loadedPopup = await _addressablesLoader.LoadAsync<GameObject>(popupReference, cancellationToken);
                var createdWindow = _objectResolver.Instantiate(loadedPopup, _popupsContainer);

                if (!createdWindow.TryGetComponent<PopupBase>(out var popupBase))
                    continue;

                _createdPopups[popupType] = popupBase;

                popupBase.BaseInitialize();
                popupBase.Initialize();
                popupBase.HideImmediately();
                popupBase.OnHidden.SubscribeUntilDestroy(this, static self => self.HandlePopupHide());
            }
        }

        public void OpenScreenByType(TScreenEnum screenType)
        {
            HideAllPopups();

            var screenBase = _createdScreens[screenType];

            if (!screenBase)
            {
                var message = StringFormatter.Concat("[WindowsControllerBase::OpenScreenByType] " +
                                                     "There is no screen with type: ", screenType);
                Debug.LogError(message);
                return;
            }

            if (_currentScreen)
                _currentScreen.HideAsync();

            _currentScreen = screenBase;
            _currentScreenType.Value = screenType;
            screenBase.ShowAsync();
        }

        public void OpenPopupByType(TPopupEnum popupType)
        {
            var popupBase = _createdPopups[popupType];

            if (!popupBase)
            {
                var message = StringFormatter.Concat("[WindowsControllerBase::OpenPopupByType] " +
                                                     "There is no popup with type: ", popupType);
                Debug.LogError(message);
                return;
            }

            OpenPopupAsync(popupBase, popupType).Forget();
        }

        public void OpenScreen<TParameters>(TScreenEnum screenType, TParameters parameters)
        {
            var screenBase = _createdScreens[screenType];

            // ReSharper disable once SuspiciousTypeConversion.Global | will be inherited on the user side
            // ReSharper disable once Unity.NoNullPatternMatching | checked on the left
            if (!screenBase || screenBase is not IParameterizedWindow<TParameters> parameterizedScreen)
            {
                Debug.LogError("[WindowsControllerBase::OpenScreen] " +
                               $"Screen {screenType} does not implement {nameof(IParameterizedWindow<TParameters>)}");
                return;
            }

            parameterizedScreen.SetParameters(parameters);
            OpenScreenByType(screenType);
        }

        public void OpenPopup<TParameters>(TPopupEnum popupType, TParameters parameters)
        {
            var popupBase = _createdPopups[popupType];

            // ReSharper disable once SuspiciousTypeConversion.Global | will be inherited on the user side
            // ReSharper disable once Unity.NoNullPatternMatching | checked on the left
            if (!popupBase || popupBase is not IParameterizedWindow<TParameters> parameterizedPopup)
            {
                Debug.LogError("[WindowsControllerBase::OpenPopup] " +
                               $"Popup {popupType} does not implement {nameof(IParameterizedWindow<TParameters>)}");
                return;
            }

            parameterizedPopup.SetParameters(parameters);
            OpenPopupAsync(popupBase, popupType).Forget();
        }

        public void BindPopupOpen(UIBehaviour component, TPopupEnum popupType)
        {
            component.OnPointerClickAsObservable()
                .Subscribe((self: this, popupType), static (_, tuple) => tuple.self.OpenPopupByType(tuple.popupType))
                .RegisterTo(component.destroyCancellationToken);
        }

        public void BindScreenOpen(UIBehaviour component, TScreenEnum screenType)
        {
            component.OnPointerClickAsObservable()
                .Subscribe((self: this, screenType), static (_, tuple) => tuple.self.OpenScreenByType(tuple.screenType))
                .RegisterTo(component.destroyCancellationToken);
        }

        private async UniTask OpenPopupAsync(PopupBase popupBase, TPopupEnum popupEnum)
        {
            if (_currentOpenedPopup && !popupBase.IsInFrontOf(_currentOpenedPopup))
                popupBase.transform.SetAsLastSibling();

            await popupBase.ShowAsync();

            if (_currentOpenedPopup)
            {
                _previousOpenedPopups.Push(_currentOpenedPopup);

                if (popupBase.IsSingle)
                    _currentOpenedPopup.HideImmediately();
            }

            _currentOpenedPopup = popupBase;
            CurrentPopupType = popupEnum;
        }

        private void HideAllPopups()
        {
            CurrentPopupType = default;

            if (_currentOpenedPopup)
            {
                _currentOpenedPopup.HideImmediately();
                _currentOpenedPopup = null;
            }

            _previousOpenedPopups.Clear();
        }

        private void HandlePopupHide()
        {
            CurrentPopupType = default;

            var needShow = _currentOpenedPopup && _currentOpenedPopup.IsSingle;
            _currentOpenedPopup = null;

            if (!_previousOpenedPopups.TryPop(out var previousPopup))
                return;

            _currentOpenedPopup = previousPopup;
            if (needShow)
                previousPopup.ShowAsync().Forget();
        }
    }
}