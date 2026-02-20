using CustomUtils.Runtime.UI.GradientHelpers.Base;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace CustomUtils.Runtime.UI.GradientHelpers
{
    /// <summary>
    /// Provides gradient effects for TextMeshProUGUI components using built-in vertex gradient functionality.
    /// </summary>
    [PublicAPI]
    public sealed class TextGradientEffect : GradientEffectBase<TextMeshProUGUI>
    {
        protected override void ApplyGradient(
            TextMeshProUGUI text,
            Color startColor,
            Color endColor,
            GradientDirection direction)
        {
            text.colorGradient = direction switch
            {
                GradientDirection.TopToBottom => new VertexGradient(startColor, startColor, endColor, endColor),
                GradientDirection.LeftToRight => new VertexGradient(startColor, endColor, endColor, startColor),
                GradientDirection.BottomToTop => new VertexGradient(endColor, endColor, startColor, startColor),
                GradientDirection.RightToLeft => new VertexGradient(endColor, startColor, startColor, endColor),
                _ => new VertexGradient(startColor)
            };

            text.enableVertexGradient = direction != GradientDirection.None;
        }

        /// <summary>
        /// Removes any gradient effect from the specified TextMeshProUGUI component.
        /// </summary>
        /// <param name="text">The TextMeshProUGUI component to clear the gradient from.</param>
        /// <remarks>
        /// This method disables vertex gradient rendering, restoring the text to its original uniform color.
        /// </remarks>
        public override void ClearGradient(TextMeshProUGUI text)
        {
            text.enableVertexGradient = false;
        }
    }
}