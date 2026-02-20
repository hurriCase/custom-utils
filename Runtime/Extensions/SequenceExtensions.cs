using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Sequence"/>.
    /// </summary>
    [PublicAPI]
    public static class SequenceExtensions
    {
        /// <summary>
        /// Creates a sequence that simultaneously tweens the RectTransform's anchored position and rotation.
        /// </summary>
        /// <param name="rectTransform">The RectTransform to animate.</param>
        /// <param name="targetPosition">The target anchored position to tween to.</param>
        /// <param name="targetRotation">The target rotation to tween to.</param>
        /// <param name="duration">The duration of both animations in seconds.</param>
        /// <returns>A PrimeTween Sequence that animates position and rotation simultaneously.</returns>
        public static Sequence TweenPositionAndRotation(
            this RectTransform rectTransform,
            Vector2 targetPosition,
            Quaternion targetRotation,
            float duration) =>
            Sequence.Create()
                .Chain(Tween.UIAnchoredPosition(rectTransform, targetPosition, duration))
                .Group(Tween.Rotation(rectTransform, targetRotation, duration));

        /// <summary>
        /// Creates a sequence that resets the RectTransform to its original state by simultaneously
        /// tweening position to the specified original position, rotation to identity, and scale to Vector3.one.
        /// </summary>
        /// <param name="rectTransform">The RectTransform to reset.</param>
        /// <param name="originalPosition">The original anchored position to return to.</param>
        /// <param name="duration">The duration of all animations in seconds.</param>
        /// <returns>A PrimeTween Sequence that resets the transform to its original state.</returns>
        public static Sequence TweenToOriginalTransform(
            this RectTransform rectTransform,
            Vector2 originalPosition,
            float duration) =>
            Sequence.Create()
                .Chain(Tween.UIAnchoredPosition(rectTransform, originalPosition, duration))
                .Group(Tween.Rotation(rectTransform, Quaternion.identity, duration))
                .Group(Tween.Scale(rectTransform, Vector3.one, duration));
    }
}