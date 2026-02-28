using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Animations.Settings;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations
{
    /// <summary>
    /// Animates the anchored position of a RectTransform based on state, within a specified axis.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class AnchoredAxisPositionAnimation<TState> : AnimationBase<TState, float, FloatAnimationSettings>
        where TState : unmanaged, Enum
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private AnimationAxis _axis;

        protected override void SetValueInstant(float value)
        {
            if (_axis == AnimationAxis.None)
                return;

            _target.anchoredPosition = _axis switch
            {
                AnimationAxis.X => new Vector2(value, _target.anchoredPosition.y),
                AnimationAxis.Y => new Vector2(_target.anchoredPosition.x, value),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected override Tween CreateTween(FloatAnimationSettings animationSettings)
        {
            if (_axis == AnimationAxis.None)
                return Tween.Delay(0f);

            return _axis switch
            {
                AnimationAxis.X => Tween.UIAnchoredPositionX(_target, animationSettings.TweenSettings),
                AnimationAxis.Y => Tween.UIAnchoredPositionY(_target, animationSettings.TweenSettings),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}