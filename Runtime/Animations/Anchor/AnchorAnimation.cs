using System;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Animations.Settings;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Anchor
{
    /// <summary>
    /// Animates the anchorMin and anchorMax of a RectTransform based on state.
    /// xy = anchorMin, zw = anchorMax.
    /// </summary>
    /// <typeparam name="TState">The enum type representing animation states.</typeparam>
    [PublicAPI]
    [Serializable]
    public sealed class AnchorAnimation<TState> : AnimationBase<TState, Vector4, Vector4AnimationSettings>
        where TState : unmanaged, Enum
    {
        [SerializeField] private RectTransform _target;

        protected override void SetValueInstant(Vector4 value)
        {
            _target.anchorMin = new Vector2(value.x, value.y);
            _target.anchorMax = new Vector2(value.z, value.w);
        }

        protected override Tween CreateTween(Vector4AnimationSettings animationSettings)
            => Tween.Custom(
                this,
                animationSettings.TweenSettings,
                static (self, value) => self.SetValueInstant(value));
    }
}