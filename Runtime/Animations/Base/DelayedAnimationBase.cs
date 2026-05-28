using System;
using CustomUtils.Runtime.Animations.Base.Settings;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Base
{
    [Serializable]
    public abstract class DelayedAnimationBase<TTarget, TContent, TState> : StatefulAnimationBase<TState>
        where TState : unmanaged, Enum
    {
        [SerializeField] protected TTarget target;
        [SerializeField] private DelayedAnimationSettingsBase<TState, TContent> _animationSettings;

        protected TContent targetSource;

        protected override Tween OnPlayAnimation(TState state, bool isInstant)
        {
            if (isInstant && _animationSettings.SkipWhenInstant)
                return default;

            targetSource = _animationSettings.States[state];

            if (isInstant)
            {
                UpdateState();
                return default;
            }

            if (CurrentAnimation.isAlive)
                CurrentAnimation.Stop();

            return Tween.Delay(
                this,
                _animationSettings.Delay,
                static self => self.UpdateState(),
                _animationSettings.UseUnscaledTime);
        }

        protected abstract void UpdateState();
    }
}