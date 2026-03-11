using System;
using System.Threading;
using CustomUtils.Runtime.UI.Windows.Windows.Parameterized;
using CustomUtils.Runtime.UI.Windows.Windows.Plain;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine.EventSystems;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    public interface IWindowsController
    {
        ReadOnlyReactiveProperty<Type> CurrentScreenType { get; }
        Type CurrentPopupType { get; }
        UniTask InitializeAsync(CancellationToken cancellationToken);
        void OpenScreen<TScreen>() where TScreen : ScreenBase;

        void OpenScreen<TParameterizedScreen, TParameters>(TParameters parameters)
            where TParameterizedScreen : ParameterizedScreenBase<TParameters>;

        void OpenPopup<TPopup>() where TPopup : PopupBase;

        void OpenPopup<TParameterizedPopup, TParameters>(TParameters parameters)
            where TParameterizedPopup : ParameterizedPopupBase<TParameters>;

        void BindScreenOpen<TScreen>(UIBehaviour component) where TScreen : ScreenBase;
        void BindPopupOpen<TPopup>(UIBehaviour component) where TPopup : PopupBase;
    }
}