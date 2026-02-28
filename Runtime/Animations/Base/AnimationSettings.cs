using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Animations.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Contains the target value and tween settings for an animation.
    /// </summary>
    /// <typeparam name="TValue">The type of value being animated.</typeparam>
    [PublicAPI]
    public abstract class AnimationSettings<TValue> : ScriptableObject
        where TValue : struct
    {
        [field: SerializeField] public TweenSettings<TValue> TweenSettings { get; private set; }

        protected const string AnimationSettingsPath = "Animation Settings/";
    }
}