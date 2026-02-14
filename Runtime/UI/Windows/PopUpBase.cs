using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.CustomComponents.Selectables.Buttons;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    public abstract class PopUpBase : WindowBase
    {
        [field: SerializeField] internal bool IsSingle { get; private set; } = true;

        [SerializeField] private PopUpVisibilityHandler _popUpVisibilityHandler;

        [SerializeField] private ThemeButton _closeButton;

        public Observable<Unit> OnPopUpShown => _popUpShown;
        private readonly Subject<Unit> _popUpShown = new();

        public Observable<Unit> OnPopUpHidden => _popUpHidden;
        private readonly Subject<Unit> _popUpHidden = new();

        internal override void BaseInitialize()
        {
            _closeButton.AsNullable()?.OnClickAsObservable()
                .SubscribeUntilDestroy(this, static self => self.HideAsync().Forget());
        }

        public override async UniTask ShowAsync()
        {
            await _popUpVisibilityHandler.ShowAsync();

            _popUpShown.OnNext(Unit.Default);
        }

        public override async UniTask HideAsync()
        {
            await _popUpVisibilityHandler.HideAsync();

            _popUpHidden.OnNext(Unit.Default);
        }

        public override void HideImmediately()
        {
            _popUpVisibilityHandler.HideImmediately();

            _popUpHidden.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _popUpHidden?.Dispose();
        }
    }
}