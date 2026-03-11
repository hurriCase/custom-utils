using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.Windows.Windows;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;
using VContainer;

namespace CustomUtils.Runtime.UI.Windows.Registries
{
    [PublicAPI]
    internal sealed class PopupRegistry : WindowRegistry<PopupBase>
    {
        public Type CurrentPopupType { get; private set; }

        private readonly Stack<PopupBase> _previousOpenedPopups = new();

        private CancellationToken _token;

        internal PopupRegistry(
            Transform container,
            IObjectResolver objectResolver,
            IAddressablesLoader addressablesLoader,
            CancellationToken token)
            : base(container, objectResolver, addressablesLoader)
        {
            _token = token;
        }

        protected override void OnRegistered(PopupBase popupBase)
        {
            popupBase.HideImmediately();
            popupBase.OnHidden
                .SubscribeSelf(this, static self => self.HandlePopupHide())
                .RegisterTo(_token);
        }

        protected override async UniTaskVoid OpenWindow(PopupBase popupBase)
        {
            if (currentWindow && !popupBase.IsInFrontOf(currentWindow))
                popupBase.transform.SetAsLastSibling();

            await popupBase.ShowAsync();

            if (currentWindow)
            {
                _previousOpenedPopups.Push(currentWindow);

                if (popupBase.IsSingle)
                    currentWindow.HideImmediately();
            }

            currentWindow = popupBase;
            CurrentPopupType = popupBase.GetType();
        }

        internal void HideAll()
        {
            CurrentPopupType = null;
            HideCurrent();
            _previousOpenedPopups.Clear();
        }

        internal void HandlePopupHide()
        {
            CurrentPopupType = null;

            var needShow = currentWindow && currentWindow.IsSingle;
            currentWindow = null;

            if (!_previousOpenedPopups.TryPop(out var previousPopup))
                return;

            currentWindow = previousPopup;
            if (needShow)
                previousPopup.ShowAsync().Forget();
        }
    }
}