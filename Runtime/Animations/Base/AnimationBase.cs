using System;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.CustomTypes.Collections;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Base
{
    [Serializable]
    public abstract class AnimationBase<TState, TValue, TAnimationSettings> : IAnimation<TState>
        where TState : unmanaged, Enum
        where TAnimationSettings : AnimationSettings<TValue>
        where TValue : struct
    {
        [SerializeField] protected EnumArray<TState, TAnimationSettings> states;

#if UNITY_EDITOR

        // ReSharper disable once NotAccessedField.Local | only for display purposes
        [SerializeField, InspectorReadOnly] private TState _currentState;
#endif

        private Tween _currentAnimation;

        public Tween PlayAnimation(TState state, bool isInstant = false)
        {
            var currentState = states[state];

#if UNITY_EDITOR
            _currentState = state;
#endif

            if (isInstant)
            {
                SetValueInstant(currentState.TweenSettings.endValue);
                return default;
            }

            if (_currentAnimation.isAlive)
                _currentAnimation.Stop();

            return _currentAnimation = CreateTween(currentState);
        }

        public void CancelAnimation()
        {
            _currentAnimation.Stop();
        }

        protected abstract void SetValueInstant(TValue value);
        protected abstract Tween CreateTween(TAnimationSettings animationSettings);
    }
}