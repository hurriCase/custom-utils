using System.Collections.Generic;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using PrimeTween;

namespace CustomUtils.Runtime.UI.Windows
{
    internal sealed class WindowPose
    {
        internal bool IsHideOneWay => _hideAnimation && !_hideAnimation.animation.isReversible;

        internal TweenAnimation ReturningAnimation => _hideAnimation ? _hideAnimation.animation : _animation;

        private readonly TweenAnimation _animation;
        private readonly TweenAnimationComponent _hideAnimation;
        private readonly IReadOnlyList<SharedElement> _sharedElements;
        private readonly WindowMode _mode;
        private readonly float _blendHideDuration;
        private readonly float _reverseTimeScale;
        private readonly float _hideTimeScale = 1f;
        private readonly HashSet<string> _heldElementKeys = new();

        internal WindowPose(
            TweenAnimation animation,
            TweenAnimationComponent hideAnimation,
            IReadOnlyList<SharedElement> sharedElements,
            WindowMode mode,
            float blendHideDuration)
        {
            _animation = animation;
            _hideAnimation = hideAnimation;
            _sharedElements = sharedElements;
            _mode = mode;
            _blendHideDuration = blendHideDuration;
            _reverseTimeScale = CalculateReverseTimeScale(animation);

            if (hideAnimation)
                _hideTimeScale = CalculateReverseTimeScale(hideAnimation.animation);
        }

        internal TweenAnimation GetPlayingAnimation(bool isVisible)
            => isVisible || !_hideAnimation ? _animation : _hideAnimation.animation;

        internal void HoldSharedElements(IEnumerable<string> keys)
        {
            _heldElementKeys.Clear();
            _heldElementKeys.UnionWith(keys);
        }

        internal void Play(bool isVisible)
        {
            if (_hideAnimation && !isVisible)
            {
                PlayHide();
                return;
            }

            if (_hideAnimation)
            {
                _hideAnimation.animation.Reset();
                _animation.SetStateInstantly(false);
            }

            var timeScale = isVisible ? 1f : _reverseTimeScale;
            _animation.PlayToState(isVisible, timeScale);
            PlaySharedElements(isVisible);
        }

        internal void Return()
        {
            ReturnSharedElements();

            if (_hideAnimation)
            {
                _hideAnimation.animation.state = false;
                return;
            }

            _animation.PlayToState(true, _reverseTimeScale);
        }

        internal void SetInstantly(bool isVisible)
        {
            if (_hideAnimation)
                _hideAnimation.animation.Reset();

            _animation.SetStateInstantly(isVisible);

            foreach (var element in _sharedElements)
                element.Animation.animation.SetStateInstantly(isVisible);

            _heldElementKeys.Clear();
        }

        internal void SpeedUp(float timeScale, bool isVisible)
        {
            var poseTimeScale = isVisible ? timeScale : timeScale * _reverseTimeScale;
            _animation.SetTimeScaleKeepingDirection(poseTimeScale);

            foreach (var element in _sharedElements)
            {
                var elementAnimation = element.Animation.animation;
                var elementReverseTimeScale = CalculateReverseTimeScale(elementAnimation);
                var elementTimeScale = isVisible ? timeScale : timeScale * elementReverseTimeScale;
                elementAnimation.SetTimeScaleKeepingDirection(elementTimeScale);
            }

            if (!_hideAnimation)
                return;

            var hideTimeScale = timeScale * _hideTimeScale;
            _hideAnimation.animation.SetTimeScaleKeepingDirection(hideTimeScale);
        }

        private void PlayHide()
        {
            if (_animation.isAlive)
                _animation.Stop();

            PlaySharedElements(false);
            StartHideAnimation();
        }

        private void PlaySharedElements(bool isVisible)
        {
            foreach (var element in _sharedElements)
            {
                var elementAnimation = element.Animation.animation;
                if (_heldElementKeys.Contains(element.Key))
                {
                    if (isVisible)
                        elementAnimation.SetStateInstantly(true);

                    continue;
                }

                if (_hideAnimation && isVisible)
                    elementAnimation.SetStateInstantly(false);

                var timeScale = isVisible ? 1f : CalculateReverseTimeScale(elementAnimation);
                elementAnimation.PlayToState(isVisible, timeScale);
            }

            _heldElementKeys.Clear();
        }

        private void ReturnSharedElements()
        {
            foreach (var element in _sharedElements)
            {
                var elementAnimation = element.Animation.animation;
                if (_heldElementKeys.Contains(element.Key))
                {
                    elementAnimation.SetStateInstantly(true);
                    continue;
                }

                var timeScale = CalculateReverseTimeScale(elementAnimation);
                elementAnimation.PlayToState(true, timeScale);
            }

            _heldElementKeys.Clear();
        }

        private void StartHideAnimation()
        {
            var hideAnimation = _hideAnimation.animation;
            hideAnimation.Reset();
            hideAnimation.timeScale = _hideTimeScale;
            hideAnimation.state = true;
        }

        private float CalculateReverseTimeScale(TweenAnimation animation)
        {
            if (_mode != WindowMode.Blend)
                return 1f;

            var duration = animation.CalculateDuration();
            return duration > 0f ? duration / _blendHideDuration : 1f;
        }
    }
}
