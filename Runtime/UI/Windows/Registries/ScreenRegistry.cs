using System;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.UI.Windows.Windows;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;
using VContainer;

namespace CustomUtils.Runtime.UI.Windows.Registries
{
    [PublicAPI]
    internal sealed class ScreenRegistry : WindowRegistry<ScreenBase>
    {
        private readonly ReactiveProperty<Type> _currentScreenType;

        internal ScreenRegistry(
            ReactiveProperty<Type> currentScreenType,
            Transform container,
            IObjectResolver objectResolver,
            IAddressablesLoader addressablesLoader)
            : base(container, objectResolver, addressablesLoader)
        {
            _currentScreenType = currentScreenType;
        }

        protected override void OnRegistered(ScreenBase screenBase)
        {
            if (screenBase.InitialWindow)
            {
                _currentScreenType.Value = screenBase.GetType();
                return;
            }

            screenBase.HideImmediately();
        }

        protected override UniTaskVoid OpenWindow(ScreenBase screenBase)
        {
            if (currentWindow)
                currentWindow.HideAsync();

            currentWindow = screenBase;
            _currentScreenType.Value = screenBase.GetType();
            screenBase.ShowAsync();
            return default;
        }
    }
}