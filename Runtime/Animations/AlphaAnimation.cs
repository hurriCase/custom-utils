using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Animations.Settings;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations
{
    /// <summary>
    /// Animates the alpha value of a CanvasGroup based on state.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class AlphaAnimation<TState> : AnimationBase<TState, float, FloatAnimationSettings>
        where TState : unmanaged, Enum
    {
        [SerializeField] private CanvasGroup _target;

        private float _targetAlpha;

        protected override void SetValueInstant(float value)
        {
            _target.alpha = value;
            _target.interactable = value > 0;
            _target.blocksRaycasts = value > 0;
        }

        protected override Tween CreateTween(FloatAnimationSettings animationSettings)
        {
            _targetAlpha = animationSettings.Value;
            return Tween.Alpha(_target, animationSettings.Value, animationSettings.TweenSettings)
                .OnComplete(this, static self => self.SetValueInstant(self._targetAlpha));
        }
    }
}