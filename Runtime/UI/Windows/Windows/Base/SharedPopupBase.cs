using System.ComponentModel;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Windows.Windows.Base
{
    [PublicAPI]
    [RequireComponent(typeof(VisibilityHandler))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SharedPopupBase : WindowBase
    {
        [field: SerializeField] internal bool IsSingle { get; private set; } = true;

        [SerializeField, Self] private VisibilityHandler _visibilityHandler;

        protected Button CloseButton => _visibilityHandler.CloseButton;

        public Observable<Unit> OnShown => _shown;
        private readonly Subject<Unit> _shown = new();

        public Observable<Unit> OnHidden => _hidden;
        private readonly Subject<Unit> _hidden = new();

        public override void BaseInitialize()
        {
            CloseButton.AsNullable()?.OnClickAsObservable()
                .SubscribeUntilDestroy(this, static self => self.HideAsync().Forget());
        }

        public override async UniTask ShowAsync()
        {
            canvasGroup.Show();

            await _visibilityHandler.ShowAsync();

            _shown.OnNext(Unit.Default);
        }

        public override async UniTask HideAsync()
        {
            await _visibilityHandler.HideAsync();

            canvasGroup.Hide();

            _hidden.OnNext(Unit.Default);
        }

        public override void HideImmediately()
        {
            _visibilityHandler.HideImmediately();

            canvasGroup.Hide();

            _hidden.OnNext(Unit.Default);
        }

        protected virtual void OnDestroy()
        {
            _shown.Dispose();
            _hidden.Dispose();
        }
    }
}