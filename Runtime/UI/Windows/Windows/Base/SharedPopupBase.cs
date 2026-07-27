using System.ComponentModel;
using System.Threading;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;

#if PRIMETWEEN_INSTALLED
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions.Observables;
using UnityEngine.UI;
#endif

namespace CustomUtils.Runtime.UI.Windows.Windows.Base
{
    [PublicAPI]
#if PRIMETWEEN_INSTALLED
    [RequireComponent(typeof(VisibilityHandler))]
#endif
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SharedPopupBase : WindowBase
    {
        [field: SerializeField] internal bool IsSingle { get; private set; }

#if PRIMETWEEN_INSTALLED
        [SerializeField, Self] private VisibilityHandler _visibilityHandler;

        protected Button CloseButton => _visibilityHandler.CloseButton;
#endif

        public Observable<Unit> OnShown => _shown;
        private readonly Subject<Unit> _shown = new();

        public Observable<SharedPopupBase> OnHidden => _hidden;
        private readonly Subject<SharedPopupBase> _hidden = new();

        public override void BaseInitialize()
        {
#if PRIMETWEEN_INSTALLED
            CloseButton.AsNullable()?.OnClickAsObservable()
                .SubscribeAwaitExclusive(this, static async (self, token) => await self.HideAsync(token));
#endif
        }

        public override async UniTask ShowAsync(CancellationToken token)
        {
            canvasGroup.Show();

#if PRIMETWEEN_INSTALLED
            await _visibilityHandler.ShowAsync(token);
#endif

            _shown.OnNext(Unit.Default);
        }

        public override async UniTask HideAsync(CancellationToken token)
        {
#if PRIMETWEEN_INSTALLED
            await _visibilityHandler.HideAsync(token);
#endif

            canvasGroup.Hide();

            _hidden.OnNext(this);
        }

        public override void HideImmediately()
        {
#if PRIMETWEEN_INSTALLED
            _visibilityHandler.HideImmediately();
#endif

            canvasGroup.Hide();

            _hidden.OnNext(this);
        }

        protected virtual void OnDestroy()
        {
            _shown.Dispose();
            _hidden.Dispose();
        }
    }
}