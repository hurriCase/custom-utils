using System;
using System.ComponentModel;
using System.Threading;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;
using UnityEngine.UI;

#if PRIMETWEEN_INSTALLED
using CustomUtils.Runtime.Extensions.Observables;
#endif

namespace CustomUtils.Runtime.UI.Windows.Windows.Base
{
    [PublicAPI]
    [Serializable]
    [RequireComponent(typeof(CanvasGroup), typeof(Canvas), typeof(GraphicRaycaster))]
#if PRIMETWEEN_INSTALLED
    [RequireComponent(typeof(VisibilityHandler))]
#endif
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class WindowBase : MonoBehaviour
    {
        [field: SerializeField] internal bool InitialWindow { get; private set; }

        [field: SerializeField] internal WindowMode Mode { get; private set; }

        [SerializeField, Self] protected CanvasGroup canvasGroup;
        [SerializeField, Self] private Canvas _canvas;

#if PRIMETWEEN_INSTALLED
        [SerializeField, Self] private VisibilityHandler _visibilityHandler;

        protected Button CloseButton => _visibilityHandler.CloseButton;
#endif

        public Observable<Unit> OnShown => _shown;
        private readonly Subject<Unit> _shown = new();

        public Observable<WindowBase> OnHidden => _hidden;
        private readonly Subject<WindowBase> _hidden = new();

        internal Observable<WindowBase> OnHiding => _hiding;
        private readonly Subject<WindowBase> _hiding = new();

        internal bool IsVisible { get; private set; }

        public virtual void BaseInitialize()
        {
#if PRIMETWEEN_INSTALLED
            CloseButton.AsNullable()?.OnClickAsObservable()
                .SubscribeAwaitExclusive(this, static async (self, token) => await self.HideAsync(token));
#endif
        }

        public virtual void Initialize() { }

        public virtual async UniTask ShowAsync(CancellationToken token)
        {
            var wasVisible = IsVisible;
            IsVisible = true;
            SetVisible(true);

#if PRIMETWEEN_INSTALLED
            if (!wasVisible)
                await _visibilityHandler.ShowAsync(token);
#endif

            _shown.OnNext(Unit.Default);
        }

        public virtual async UniTask HideAsync(CancellationToken token)
        {
            _hiding.OnNext(this);

            await PlayHideAnimationAsync(token);

            _hidden.OnNext(this);
        }

        public async UniTask ToggleVisibility(bool isVisible, CancellationToken token)
        {
            if (isVisible)
            {
                await ShowAsync(token);
                return;
            }

            await HideAsync(token);
        }

        public virtual void HideImmediately(bool isSilent = false)
        {
            if (!isSilent)
                _hiding.OnNext(this);

            IsVisible = false;

#if PRIMETWEEN_INSTALLED
            _visibilityHandler.HideImmediately();
#endif

            SetVisible(false);

            if (!isSilent)
                _hidden.OnNext(this);
        }

        internal void Uncover()
        {
            SetVisible(true);
        }

        internal async UniTask PlayHideAnimationAsync(CancellationToken token)
        {
            var wasVisible = IsVisible;
            IsVisible = false;

#if PRIMETWEEN_INSTALLED
            if (wasVisible)
                await _visibilityHandler.HideAsync(token);
#endif

            SetVisible(false);
        }

        protected void SetVisible(bool isVisible)
        {
            _canvas.enabled = isVisible;
            canvasGroup.SetVisible(isVisible);
        }

        protected virtual void OnDestroy()
        {
            _shown.Dispose();
            _hidden.Dispose();
            _hiding.Dispose();
        }
    }
}
