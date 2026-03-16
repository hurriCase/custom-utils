using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.GradientHelpers.Base;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.GradientHelpers.GraphicGradient.Multi
{
    /// <summary>
    /// Provides multi-stop gradient effects for Unity UI Graphic components using vertex manipulation.
    /// </summary>
    [PublicAPI]
    public sealed class MultiGradientEffect : IGradientEffect<Graphic>
    {
        /// <summary>
        /// Applies a multi-stop gradient effect to the specified Graphic component using vertex manipulation.
        /// </summary>
        /// <param name="graphic">The Graphic component to apply the gradient to.</param>
        /// <param name="gradient">The gradient containing multiple color stops to evaluate per vertex.</param>
        /// <param name="direction">The direction in which the gradient should be applied.</param>
        public void ApplyGradient(Graphic graphic, Gradient gradient, GradientDirection direction)
        {
            var effect = graphic.GetOrAddComponent<VertexMultiGradientEffect>();
            effect.SetGradient(gradient, direction, graphic.rectTransform.rect);
            graphic.SetVerticesDirty();
        }

        /// <summary>
        /// Removes any multi-stop gradient effect from the specified Graphic component.
        /// </summary>
        /// <param name="graphic">The Graphic component to clear the gradient from.</param>
        /// <remarks>
        /// This method destroys the VertexMultiGradientEffect component if present
        /// and marks the graphic's vertices as dirty for re-rendering.
        /// </remarks>
        public void ClearGradient(Graphic graphic)
        {
            if (!graphic.TryGetComponent<VertexMultiGradientEffect>(out var effect))
                return;

            effect.Destroy();
            graphic.SetVerticesDirty();
        }
    }
}