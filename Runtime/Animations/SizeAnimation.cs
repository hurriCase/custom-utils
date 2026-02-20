using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Animations.Settings;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations
{
    /// <summary>
    /// Animates the size delta of a RectTransform based on state.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class SizeAnimation<TState> : AnimationBase<TState, Vector3, Vector3AnimationSettings>
        where TState : unmanaged, Enum
    {
        [SerializeField] private RectTransform _target;

        protected override void SetValueInstant(Vector3 value)
        {
            _target.sizeDelta = value;
        }

        protected override Tween CreateTween(Vector3AnimationSettings animationSettings)
            => Tween.UISizeDelta(_target, animationSettings.Value, animationSettings.TweenSettings);
    }
}