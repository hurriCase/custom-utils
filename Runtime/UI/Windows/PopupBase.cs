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
    public abstract class PopupBase : WindowBase
    {
        [field: SerializeField] internal bool IsSingle { get; private set; } = true;

        [SerializeField] private VisibilityHandler _visibilityHandler;

        [SerializeField] private ThemeButton _closeButton;

        public Observable<Unit> OnShown => _shown;
        private readonly Subject<Unit> _shown = new();

        public Observable<Unit> OnHidden => _hidden;
        private readonly Subject<Unit> _hidden = new();

        internal override void BaseInitialize()
        {
            _closeButton.AsNullable()?.OnClickAsObservable()
                .SubscribeUntilDestroy(this, static self => self.HideAsync().Forget());
        }

        public override async UniTask ShowAsync()
        {
            await _visibilityHandler.ShowAsync();

            _shown.OnNext(Unit.Default);
        }

        public override async UniTask HideAsync()
        {
            await _visibilityHandler.HideAsync();

            _hidden.OnNext(Unit.Default);
        }

        public override void HideImmediately()
        {
            _visibilityHandler.HideImmediately();

            _hidden.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _hidden?.Dispose();
        }
    }
}