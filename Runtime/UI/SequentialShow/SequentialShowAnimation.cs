#if PRIMETWEEN_PRO_INSTALLED
using System.Threading;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.AnimationSkipping;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.SequentialShow
{
    /// <summary>
    /// Plays its content animation, then shows its <see cref="TweenSequentialShowItem"/> children and directly nested
    /// cascades row by row (or item by item), top to bottom (or bottom to top), one interval apart. Items outside the
    /// viewport of their scroll view appear at once. On hide every item plays its own hide or reverses its show.
    /// </summary>
    [PublicAPI]
    public sealed class SequentialShowAnimation : MonoBehaviour, ISkippableAnimation
    {
        [SerializeField] private TweenAnimationComponent _contentAnimation;
        [SerializeField] private float _itemsDelay;
        [SerializeField] private float _itemInterval;
        [SerializeField] private bool _reverseOnHide;
        [SerializeField] private SequentialShowOrder _order;
        [SerializeField] private bool _isBottomToTop;
        [SerializeField] private ScrollRect _scrollRect;

        private readonly SequentialShowLayout _layout = new();

        private CancellationTokenSource _playCancellation;
        private Tween _waitTween;
        private float _timeScale = 1f;
        private bool _isShown;

        public void Play(float delay)
        {
            var token = CancellationExtensions.GetFreshToken(ref _playCancellation);
            PlayAsync(delay, token).Forget();
        }

        public void ShowInstantly()
        {
            CancellationExtensions.DisposeToken(ref _playCancellation);
            SpeedUp(1f);
            _layout.Collect(this, _order, _isBottomToTop);

            if (_contentAnimation)
            {
                var contentAnimation = _contentAnimation.animation;
                contentAnimation.state = true;
                contentAnimation.Complete();
            }

            foreach (var item in _layout.Items)
                item.ShowInstantly();

            _isShown = true;
        }

        public void Hide(bool isInstant)
        {
            CancellationExtensions.DisposeToken(ref _playCancellation);
            SpeedUp(1f);

            if (isInstant)
            {
                ResetItems();
                return;
            }

            if (_reverseOnHide && _contentAnimation)
                _contentAnimation.animation.state = false;

            foreach (var item in _layout.Items)
                item.Hide(_reverseOnHide);
        }

        public void SpeedUp(float timeScale)
        {
            _timeScale = timeScale;

            if (_waitTween.isAlive)
                _waitTween.timeScale = timeScale;

            if (_contentAnimation)
                _contentAnimation.animation.SetTimeScaleKeepingDirection(timeScale);

            foreach (var item in _layout.Items)
                item.SpeedUp(timeScale);
        }

        internal void Return()
        {
            if (!_isShown)
            {
                Play(0f);
                return;
            }

            CancellationExtensions.DisposeToken(ref _playCancellation);
            SpeedUp(1f);

            if (_reverseOnHide && _contentAnimation)
                _contentAnimation.animation.state = true;

            foreach (var item in _layout.Items)
                item.Return(_reverseOnHide);
        }

        internal void ResetItems()
        {
            _isShown = false;
            _layout.Collect(this, _order, _isBottomToTop);

            if (_contentAnimation)
                _contentAnimation.animation.ResetToStart();

            foreach (var item in _layout.Items)
                item.Reset();
        }

        private async UniTask PlayAsync(float delay, CancellationToken token)
        {
            _layout.ResetScroll(_scrollRect);
            ResetItems();
            SpeedUp(1f);
            using var skippableScope = SkippableAnimations.Begin(this);

            await WaitAsync(delay, token);

            // Layout and scroll position can still settle during the delay (persisted scroll, layout rebuilds),
            // so which items are visible is decided when the cascade starts.
            _layout.Collect(this, _order, _isBottomToTop);
            var items = _layout.Items;

            // Items outside the viewport would cross the screen on their way in, so they snap into place instead.
            // Snapping them before the cascade starts would show them while an unmasked ancestor is still sliding in.
            foreach (var item in items)
            {
                if (!item.IsInViewport)
                    item.ShowInstantly();
            }

            if (_contentAnimation)
                _contentAnimation.animation.state = true;

            await WaitAsync(_itemsDelay, token);

            var previousStep = 0;
            var lastShownIndex = -1;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!item.IsInViewport)
                    continue;

                var stepsDuration = (item.Step - previousStep) * _itemInterval;
                await WaitAsync(stepsDuration, token);

                previousStep = item.Step;
                item.Show(_timeScale);
                lastShownIndex = i;
            }

            if (lastShownIndex >= 0)
            {
                var lastItem = items[lastShownIndex];
                await lastItem.WaitUntilShownAsync(token);
            }

            _isShown = true;
        }

        private async UniTask WaitAsync(float duration, CancellationToken token)
        {
            if (duration <= 0f)
                return;

            _waitTween = Tween.Delay(duration, useUnscaledTime: true);
            _waitTween.timeScale = _timeScale;
            await _waitTween.ToYieldInstruction().ToUniTask(cancellationToken: token);
        }

        private void OnDestroy()
        {
            CancellationExtensions.DisposeToken(ref _playCancellation);
        }
    }
}
#endif
