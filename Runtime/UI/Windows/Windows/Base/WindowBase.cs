using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;
using UnityEngine.UI;
using ZLinq;

namespace CustomUtils.Runtime.UI.Windows.Windows.Base
{
    /// <summary>
    /// Base for every window a <see cref="WindowsController"/> opens: a canvas whose show and hide its
    /// <see cref="VisibilityHandler"/> plays.
    /// </summary>
    [PublicAPI]
    [Serializable]
    [RequireComponent(typeof(CanvasGroup), typeof(Canvas), typeof(GraphicRaycaster))]
    [RequireComponent(typeof(VisibilityHandler))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class WindowBase : MonoBehaviour
    {
        /// <summary>
        /// Opening this window closes every window of its controller and puts it at the bottom of the stack.
        /// </summary>
        [field: SerializeField] public bool ClearsStack { get; private set; }

        [field: SerializeField] internal bool InitialWindow { get; private set; }

        [SerializeField, Self] protected CanvasGroup canvasGroup;
        [SerializeField, Self] private Canvas _canvas;

        [SerializeField, Self] private VisibilityHandler _visibilityHandler;

        internal WindowMode Mode => _visibilityHandler.Mode;

        internal IReadOnlyList<SharedElement> SharedElements => _visibilityHandler.SharedElements;

        protected IReadOnlyList<Button> CloseButtons => _visibilityHandler.CloseButtons;

        protected Observable<Unit> OnCloseClicked { get; private set; }

        /// <summary>
        /// True while this window is the top window of its controller and nothing covers it.
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsFocused => _isFocused;
        private readonly ReactiveProperty<bool> _isFocused = new();

        /// <summary>
        /// Fires when a closed window starts to show, before its show animation; not when it returns from under a
        /// covering window.
        /// </summary>
        public Observable<Unit> OnOpening => _opening;
        private readonly Subject<Unit> _opening = new();

        /// <summary>
        /// Fires when the show finishes.
        /// </summary>
        public Observable<Unit> OnShown => _shown;
        private readonly Subject<Unit> _shown = new();

        /// <summary>
        /// Fires with this window once it has closed and left the screen.
        /// </summary>
        public Observable<WindowBase> OnHidden => _hidden;
        private readonly Subject<WindowBase> _hidden = new();

        internal Observable<WindowBase> OnHiding => _hiding;
        private readonly Subject<WindowBase> _hiding = new();

        /// <summary>
        /// True from the moment the window starts to show until it starts to hide.
        /// </summary>
        public bool IsVisible => State is WindowState.Opening or WindowState.Open or WindowState.Returning;

        internal WindowState State { get; private set; }

        internal bool IsCovering => Mode != WindowMode.Overlay;

        internal bool IsSlideOver => Mode == WindowMode.SlideOver;

        /// <summary>
        /// Called once after the window is created, before <see cref="Initialize"/>. Subscribes the close buttons.
        /// </summary>
        public virtual void BaseInitialize()
        {
            OnCloseClicked =
                CloseButtons.Select(static closeButton => closeButton.OnClickAsObservable()).ToArray().Merge();
            OnCloseClicked.SubscribeAwaitExclusive(this, static async (self, token) => await self.HandleCloseClickAsync(token));
        }

        /// <summary>
        /// Called once after the window is created and injected. Override it to set up the window.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Brings the window on screen. A closed window plays its show; a window that is hiding or hidden under a
        /// covering window plays its hide backwards. Override it to refresh the content each time the window appears.
        /// </summary>
        public virtual async UniTask ShowAsync(CancellationToken token)
        {
            if (IsVisible)
                return;

            var isReturning = State is WindowState.Covering or WindowState.Covered;
            var showingState = isReturning ? WindowState.Returning : WindowState.Opening;
            State = showingState;
            SetVisible(true);

            if (!isReturning)
                _opening.OnNext(Unit.Default);

            var showTask = isReturning ? _visibilityHandler.ReturnAsync(token) : _visibilityHandler.ShowAsync(token);
            await showTask;

            if (State != showingState)
                return;

            State = WindowState.Open;
            _shown.OnNext(Unit.Default);
        }

        /// <summary>
        /// Closes the window: its controller plays the hide and shows the windows it uncovers.
        /// Completes once the window is closed, or when it is opened again before its hide finishes.
        /// </summary>
        public virtual async UniTask HideAsync(CancellationToken token)
        {
            if (State == WindowState.Closed)
                return;

            if (State != WindowState.Closing)
                _hiding.OnNext(this);

            var closedOrReopened = OnHidden.AsUnitObservable().Merge(_opening);
            await closedOrReopened.FirstAsync(token);
        }

        /// <summary>
        /// Completes once the window has closed, at once when it is already closed.
        /// </summary>
        public async UniTask WaitUntilClosedAsync(CancellationToken token)
        {
            if (State == WindowState.Closed)
                return;

            await OnHidden.FirstAsync(token);
        }

        /// <summary>
        /// Shows the window when <paramref name="isVisible"/> is true, otherwise closes it.
        /// </summary>
        public async UniTask ToggleVisibility(bool isVisible, CancellationToken token)
        {
            if (isVisible)
            {
                await ShowAsync(token);
                return;
            }

            await HideAsync(token);
        }

        /// <summary>
        /// Closes the window at once, without animations. Unless <paramref name="isSilent"/> is true,
        /// <see cref="OnHidden"/> fires and the controller removes the window from its stack.
        /// </summary>
        public virtual void HideImmediately(bool isSilent = false)
        {
            State = WindowState.Closed;

            _visibilityHandler.HideImmediately();

            SetVisible(false);

            if (isSilent)
                return;

            _hidden.OnNext(this);
            _hiding.OnNext(this);
        }

        /// <summary>
        /// Plays the hide pose. Runs both when the window closes and when a window covers it.
        /// Override it to add hide effects.
        /// </summary>
        public virtual async UniTask PlayHideAnimationAsync(CancellationToken token)
        {
            await _visibilityHandler.HideAsync(token);
        }

        /// <summary>
        /// Plays the hide, when the controller hides every window at once.
        /// Override it to add effects.
        /// </summary>
        public virtual async UniTask PlayBlendOutAnimationAsync(CancellationToken token)
        {
            await _visibilityHandler.BlendOutAsync(token);
        }

        internal async UniTask StartHideAsync(bool isClosing, bool isInstant, CancellationToken token)
        {
            if (State == WindowState.Closed)
                return;

            var isOnScreen = IsVisible;
            State = isClosing ? WindowState.Closing : WindowState.Covering;

            if (isOnScreen && !isInstant)
                await PlayHideAnimationAsync(token);
        }

        internal async UniTask BlendOutAsync(CancellationToken token)
        {
            if (!IsVisible)
                return;

            State = WindowState.Covering;
            await PlayBlendOutAnimationAsync(token);
        }

        internal void CompleteHide()
        {
            if (State == WindowState.Closed)
            {
                HideCanvas();
                return;
            }

            var isClosing = State == WindowState.Closing;
            if (!isClosing && State != WindowState.Covering)
                return;

            State = isClosing ? WindowState.Closed : WindowState.Covered;
            HideCanvas();

            if (!isClosing)
                return;

            _visibilityHandler.HideImmediately();

            _hidden.OnNext(this);
        }

        internal void Reveal()
        {
            State = WindowState.Open;
            SetVisible(true);

            _visibilityHandler.Reveal();
        }

        internal void HoldSharedElements(IEnumerable<string> keys)
        {
            _visibilityHandler.HoldSharedElements(keys);
        }

        internal void SetFocused(bool isFocused)
        {
            _isFocused.Value = isFocused;
        }

        internal void ShowBackground()
        {
            SetVisible(true);
            _visibilityHandler.ShowBackground();
        }

        internal void FadeOutBackground()
        {
            _visibilityHandler.FadeOutBackground();
        }

        protected virtual async UniTask HandleCloseClickAsync(CancellationToken token)
        {
            await HideAsync(token);
        }

        protected void SetClosable(bool isClosable)
        {
            foreach (var closeButton in CloseButtons)
                closeButton.interactable = isClosable;
        }

        private void HideCanvas()
        {
            SetVisible(false);

            _visibilityHandler.HideBackground();
        }

        protected void SetVisible(bool isVisible)
        {
            _canvas.enabled = isVisible;
            canvasGroup.SetVisible(isVisible);
        }

        protected virtual void OnDestroy()
        {
            _isFocused.Dispose();
            _opening.Dispose();
            _shown.Dispose();
            _hidden.Dispose();
            _hiding.Dispose();
        }
    }
}