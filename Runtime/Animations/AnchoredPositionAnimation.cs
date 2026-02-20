using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Animations.Settings;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations
{
    /// <summary>
    /// Animates the anchored position of a RectTransform based on state, with optional axis restriction.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class AnchoredPositionAnimation<TState> : AnimationBase<TState, Vector2, Vector2AnimationSettings>
        where TState : unmanaged, Enum
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private AnimationAxis _axis = AnimationAxis.Both;

        protected override void SetValueInstant(Vector2 value)
        {
            if (_axis == AnimationAxis.None)
                return;

            _target.anchoredPosition = _axis switch
            {
                AnimationAxis.X => new Vector2(value.x, _target.anchoredPosition.y),
                AnimationAxis.Y => new Vector2(_target.anchoredPosition.x, value.y),
                _ => value
            };
        }

        protected override Tween CreateTween(Vector2AnimationSettings animationSettings)
        {
            if (_axis == AnimationAxis.None)
                return Tween.Delay(0f);

            var endValue = animationSettings.Value;
            var tweenSettings = animationSettings.TweenSettings;
            return _axis switch
            {
                AnimationAxis.X => Tween.UIAnchoredPositionX(_target, endValue.x, tweenSettings),
                AnimationAxis.Y => Tween.UIAnchoredPositionY(_target, endValue.y, tweenSettings),
                _ => Tween.UIAnchoredPosition(_target, endValue, tweenSettings)
            };
        }
    }
}