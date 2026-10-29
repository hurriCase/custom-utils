using System;
using CustomUtils.Collections.Scripts;
using CustomUtils.Runtime.Animations.Base.Settings;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Base
{
    [Serializable]
    public abstract class AnimationBase<TState, TValue, TAnimationSettings> : StatefulAnimationBase<TState>
#if UNITY_EDITOR
        , IAnimationPreview
#endif
        where TState : unmanaged, Enum
        where TAnimationSettings : TweenAnimationSettingsBase<TValue>
        where TValue : struct
    {
        [SerializeField] protected EnumArray<TState, TAnimationSettings> states;

        protected override Tween OnPlayAnimation(TState state, bool isInstant)
        {
            var currentState = states[state];

            if (CurrentAnimation.isAlive)
                CurrentAnimation.Stop();

            if (isInstant)
            {
                SetValueInstant(currentState.TweenSettings.endValue);
                return default;
            }

            // PrimeTween applies the start value in its LateUpdate, so a tween created after it (e.g. from an async
            // continuation) renders one frame at the previous value: a popup shown this way flashed before fading in
            var tweenSettings = currentState.TweenSettings;
            if (!tweenSettings.startFromCurrent)
                SetValueInstant(tweenSettings.startValue);

            return CreateTween(currentState);
        }

        protected abstract void SetValueInstant(TValue value);
        protected abstract Tween CreateTween(TAnimationSettings animationSettings);

#if UNITY_EDITOR
        public void PreviewAnimation(Enum state)
        {
            PlayAnimation((TState)state);
        }
#endif
    }
}