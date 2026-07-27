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
        public Observable<Unit> OnAllPopupClosed => _allPopupClosed;
        private readonly Subject<Unit> _allPopupClosed = new();

        private readonly List<SharedPopupBase> _previousOpenedPopups = new();

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
                .SubscribeSelf(this, static (popup, self) => self.HandlePopupHide(popup))
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
                _previousOpenedPopups.Add(currentWindow);

                if (sharedPopupBase.IsSingle)
                    currentWindow.HideImmediately();
            }

            currentWindow = sharedPopupBase;
            return sharedPopupBase;
        }

        internal void HideAll()
        {
            SetCurrentType(null);

            foreach (var previousOpenedPopup in _previousOpenedPopups)
                previousOpenedPopup.HideImmediately();

            _previousOpenedPopups.Clear();

            currentWindow = null;
        }

        private void HandlePopupHide(SharedPopupBase popup)
        {
            if (popup != currentWindow)
            {
                _previousOpenedPopups.Remove(popup);
                return;
            }

            SetCurrentType(null);

            var needShow = currentWindow && currentWindow.IsSingle;
            currentWindow = null;

            if (_previousOpenedPopups.Count == 0)
            {
                _allPopupClosed.OnNext(Unit.Default);
                return;
            }

            var lastIndex = _previousOpenedPopups.Count - 1;
            currentWindow = _previousOpenedPopups[lastIndex];
            _previousOpenedPopups.RemoveAt(lastIndex);

            if (needShow)
                currentWindow.ShowAsync(currentWindow.destroyCancellationToken).Forget();
        }
    }
}