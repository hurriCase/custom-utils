using System;
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
    internal sealed class ScreenRegistry : WindowRegistry<SharedScreenBase>
    {
        private CancellationToken _controllerCancellationToken;

        internal ScreenRegistry(
            ReactiveProperty<Type> currentScreenType,
            IAddressablesLoader addressablesLoader,
            IObjectResolver objectResolver,
            PopupRegistry popupRegistry,
            Transform container,
            CancellationToken token)
            : base(currentScreenType, container, objectResolver, addressablesLoader)
        {
            _controllerCancellationToken = token;

            popupRegistry?.OnAllPopupClosed
                .SubscribeSelf(this, token, static (token, self) => self.currentWindow.AsNullable()?.ShowAsync(token)
                    .Forget())
                .RegisterTo(token);
        }

        protected override void OnRegistered(SharedScreenBase sharedScreenBase)
        {
            if (sharedScreenBase.InitialWindow)
            {
                SetCurrentType(sharedScreenBase.GetType());
                currentWindow = sharedScreenBase;
                sharedScreenBase.ShowAsync(_controllerCancellationToken).Forget();
                return;
            }

            sharedScreenBase.HideImmediately();
        }

        protected override async UniTask<SharedScreenBase> OpenWindow(
            SharedScreenBase sharedScreenBase,
            CancellationToken token)
        {
            if (currentWindow)
                await currentWindow.HideAsync(token);

            currentWindow = sharedScreenBase;
            await sharedScreenBase.ShowAsync(token);
            return sharedScreenBase;
        }
    }
}