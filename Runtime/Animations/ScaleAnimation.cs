using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Animations.Settings;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations
{
    /// <summary>
    /// Animates the local scale of a Transform based on state.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class ScaleAnimation<TState> : AnimationBase<TState, Vector3, Vector3AnimationSettings>
        where TState : unmanaged, Enum
    {
        [SerializeField] private Transform _target;

        protected override void SetValueInstant(Vector3 value)
        {
            _target.localScale = value;
        }

        protected override Tween CreateTween(Vector3AnimationSettings animationSettings)
            => Tween.Scale(_target, animationSettings.TweenSettings);
    }
}