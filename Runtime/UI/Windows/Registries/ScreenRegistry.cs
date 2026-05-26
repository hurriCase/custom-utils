using System;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
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
        internal ScreenRegistry(
            ReactiveProperty<Type> currentScreenType,
            Transform container,
            IObjectResolver objectResolver,
            IAddressablesLoader addressablesLoader)
            : base(currentScreenType, container, objectResolver, addressablesLoader) { }

        protected override void OnRegistered(SharedScreenBase sharedScreenBase)
        {
            if (sharedScreenBase.InitialWindow)
            {
                SetCurrentType(sharedScreenBase.GetType());
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