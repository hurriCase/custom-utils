using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Animations.Settings;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations
{
    /// <summary>
    /// Animates the anchored position of a RectTransform based on state.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class AnchoredPositionAnimation<TState> : AnimationBase<TState, Vector2, Vector2AnimationSettings>
        where TState : unmanaged, Enum
    {
        [SerializeField] private RectTransform _target;

        protected override void SetValueInstant(Vector2 value)
        {
            _target.anchoredPosition = value;
        }

        protected override Tween CreateTween(Vector2AnimationSettings animationSettings)
        {
            var tweenSettings = animationSettings.TweenSettings;
            return Tween.UIAnchoredPosition(_target, tweenSettings);
        }
    }
}