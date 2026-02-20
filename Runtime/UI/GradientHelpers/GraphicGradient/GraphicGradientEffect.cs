using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.GradientHelpers.Base;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.GradientHelpers.GraphicGradient
{
    /// <inheritdoc />
    /// <summary>
    /// Provides gradient effects for Unity UI Graphic components using vertex manipulation.
    /// </summary>
    [PublicAPI]
    public sealed class GraphicGradientEffect : GradientEffectBase<Graphic>
    {
        protected override void ApplyGradient(
            Graphic graphic,
            Color startColor,
            Color endColor,
            GradientDirection direction)
        {
            var gradientEffect = graphic.GetOrAddComponent<VertexGradientEffect>();

            gradientEffect.SetGradient(startColor, endColor, direction);
            graphic.SetVerticesDirty();
        }

        /// <summary>
        /// Removes any gradient effect from the specified Graphic component.
        /// </summary>
        /// <param name="graphic">The Graphic component to clear the gradient from.</param>
        /// <remarks>
        /// This method destroys the VertexGradientEffect component if present
        /// and marks the graphic's vertices as dirty for re-rendering.
        /// </remarks>
        public override void ClearGradient(Graphic graphic)
        {
            if (graphic.TryGetComponent<VertexGradientEffect>(out var gradientEffect) is false)
                return;

            gradientEffect.Destroy();
            graphic.SetVerticesDirty();
        }
    }
}