using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Animations.Settings;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations
{
    /// <summary>
    /// Animates the color of a SpriteRenderer based on state.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class SpriteRendererColorAnimation<TState> : AnimationBase<TState, Color, ColorAnimationSettings>
        where TState : unmanaged, Enum
    {
        [SerializeField] private SpriteRenderer _target;

        protected override void SetValueInstant(Color color)
        {
            _target.color = color;
        }

        protected override Tween CreateTween(ColorAnimationSettings animationSettings)
            => Tween.Color(_target, animationSettings.TweenSettings);
    }
}