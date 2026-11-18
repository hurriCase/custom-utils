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
        private readonly float _reverseTimeScale = 1f;
        private readonly float _hideTimeScale = 1f;

        internal WindowPose(
            TweenAnimation animation,
            TweenAnimationComponent hideAnimation,
            WindowMode mode,
            float blendHideDuration)
        {
            _animation = animation;
            _hideAnimation = hideAnimation;

            if (mode != WindowMode.Blend)
                return;

            _reverseTimeScale = CalculateBlendTimeScale(animation, blendHideDuration);

            if (hideAnimation)
                _hideTimeScale = CalculateBlendTimeScale(hideAnimation.animation, blendHideDuration);
        }

        internal TweenAnimation GetPlayingAnimation(bool isVisible)
            => isVisible || !_hideAnimation ? _animation : _hideAnimation.animation;

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
        }

        internal void PlayBlendOut()
        {
            _animation.PlayToState(false, _reverseTimeScale);

            if (_hideAnimation)
                StartHideAnimation();
        }

        internal void Return()
        {
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
        }

        internal void SpeedUp(float timeScale, bool isVisible)
        {
            var poseTimeScale = isVisible ? timeScale : timeScale * _reverseTimeScale;
            _animation.SetTimeScaleKeepingDirection(poseTimeScale);

            if (!_hideAnimation)
                return;

            var hideTimeScale = timeScale * _hideTimeScale;
            _hideAnimation.animation.SetTimeScaleKeepingDirection(hideTimeScale);
        }

        private void PlayHide()
        {
            if (_animation.isAlive)
                _animation.Stop();

            StartHideAnimation();
        }

        private void StartHideAnimation()
        {
            var hideAnimation = _hideAnimation.animation;
            hideAnimation.Reset();
            hideAnimation.timeScale = _hideTimeScale;
            hideAnimation.state = true;
        }

        private float CalculateBlendTimeScale(TweenAnimation animation, float blendHideDuration)
        {
            var duration = animation.CalculateDuration();
            return duration > 0f ? duration / blendHideDuration : 1f;
        }
    }
}
