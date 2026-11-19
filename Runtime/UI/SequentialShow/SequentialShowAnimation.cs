#if PRIMETWEEN_PRO_INSTALLED
using System.Collections.Generic;
using System.Linq;
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
    /// viewport of their scroll view appear at once. With a <see cref="SequentialShowScroll"/> the scroll view starts
    /// at the cascade's first item and scrolls to its destination while the items appear. On hide every item plays its
    /// own hide or reverses its show.
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
        [SerializeField] private SequentialShowScroll _scroll;

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

            if (_scroll)
                _scroll.Complete();

            _layout.Collect(this, _order, _isBottomToTop, _scrollRect, Vector2.zero);

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

            if (_scroll)
                _scroll.Stop();

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

            if (_scroll)
                _scroll.SpeedUp(timeScale);

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
            _layout.Collect(this, _order, _isBottomToTop, _scrollRect, Vector2.zero);

            if (_contentAnimation)
                _contentAnimation.animation.ResetToStart();

            foreach (var item in _layout.Items)
                item.Reset();
        }

        private async UniTask PlayAsync(float delay, CancellationToken token)
        {
            if (_scroll)
                _scroll.Stop();

            AlignScrollToStart();
            ResetItems();
            SpeedUp(1f);
            using var skippableScope = SkippableAnimations.Begin(this);

            await WaitAsync(delay, token);

            // Layout and scroll destination can still change during the delay, so which items are visible is decided
            // when the cascade starts.
            var scrollOffset = _scroll ? _scroll.GetOffset(_scrollRect) : Vector2.zero;
            _layout.Collect(this, _order, _isBottomToTop, _scrollRect, scrollOffset);
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

            if (_scroll)
                _scroll.Play(_scrollRect.content, scrollOffset, _timeScale);

            var itemInterval = GetItemInterval(items, scrollOffset);
            var previousStep = 0;
            var lastShownIndex = -1;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!item.IsInViewport)
                    continue;

                var stepsDuration = (item.Step - previousStep) * itemInterval;
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

        // Items added in the same frame as the show (pooled items bound right before it) are not laid out yet, so the
        // content is rebuilt before it is aligned to its start.
        private void AlignScrollToStart()
        {
            if (!_scrollRect)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            _layout.ResetScroll(_scrollRect, _isBottomToTop);
        }

        // Items revealed by the scroll appear as it passes them, so the cascade is fitted into the scroll duration.
        private float GetItemInterval(IReadOnlyList<SequentialShowItem> items, Vector2 scrollOffset)
        {
            if (!_scroll || scrollOffset == Vector2.zero || items.Count == 0)
                return _itemInterval;

            var lastStep = items.Max(static item => item.Step);
            return _scroll.FitItemInterval(_itemInterval, lastStep);
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
