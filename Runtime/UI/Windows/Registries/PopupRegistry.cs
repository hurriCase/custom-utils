using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;
using VContainer;

namespace CustomUtils.Runtime.UI.Windows.Registries
{
    [PublicAPI]
    internal sealed class PopupRegistry : WindowRegistry<SharedPopupBase>
    {
        private readonly Stack<SharedPopupBase> _previousOpenedPopups = new();

        private CancellationToken _token;

        internal PopupRegistry(
            ReactiveProperty<Type> currentPopupType,
            Transform container,
            IObjectResolver objectResolver,
            IAddressablesLoader addressablesLoader,
            CancellationToken token)
            : base(currentPopupType, container, objectResolver, addressablesLoader)
        {
            _token = token;
        }

        protected override void OnRegistered(SharedPopupBase sharedPopupBase)
        {
            sharedPopupBase.HideImmediately();
            sharedPopupBase.OnHidden
                .SubscribeSelf(this, static self => self.HandlePopupHide())
                .RegisterTo(_token);
        }

        protected override async UniTask<SharedPopupBase> OpenWindow(
            SharedPopupBase sharedPopupBase,
            CancellationToken token)
        {
            if (currentWindow && !sharedPopupBase.IsInFrontOf(currentWindow))
                sharedPopupBase.transform.SetAsLastSibling();

            await sharedPopupBase.ShowAsync(token);

            if (currentWindow)
            {
                _previousOpenedPopups.Push(currentWindow);

                if (sharedPopupBase.IsSingle)
                    currentWindow.HideImmediately();
            }

            currentWindow = sharedPopupBase;
            return sharedPopupBase;
        }

        internal void HideAll()
        {
            SetCurrentType(null);
            HideCurrent();
            _previousOpenedPopups.Clear();
        }

        private void HandlePopupHide()
        {
            SetCurrentType(null);

            var needShow = currentWindow && currentWindow.IsSingle;
            currentWindow = null;

            if (!_previousOpenedPopups.TryPop(out var previousPopup))
                return;

            currentWindow = previousPopup;
            if (needShow)
                previousPopup.ShowAsync(previousPopup.destroyCancellationToken).Forget();
        }
    }
}