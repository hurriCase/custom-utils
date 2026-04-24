using System;
using CustomUtils.Runtime.Animations.Base.Settings;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Base
{
    public abstract class DelayedAnimationBase<TTarget, TContent, TState> : IAnimation<TState>
        where TState : unmanaged, Enum
    {
        [SerializeField] protected TTarget target;
        [SerializeField] private DelayedAnimationSettingsBase<TState, TContent> _animationSettings;

        private Tween _currentAnimation;

        protected TContent targetSource;

        public Tween PlayAnimation(TState state, bool isInstant)
        {
            if (isInstant && _animationSettings.SkipWhenInstant)
                return default;

            targetSource = _animationSettings.States[state];

            if (isInstant)
            {
                UpdateState();
                return default;
            }

            if (_currentAnimation.isAlive)
                _currentAnimation.Stop();

            return _currentAnimation = Tween.Delay(
                this,
                _animationSettings.Delay,
                static self => self.UpdateState(),
                _animationSettings.UseUnscaledTime);
        }

        protected abstract void UpdateState();

        public void CancelAnimation()
        {
            _currentAnimation.Stop();
        }
    }
}