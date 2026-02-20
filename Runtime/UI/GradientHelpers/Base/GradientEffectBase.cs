using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.GradientHelpers.Base
{
    [PublicAPI]
    public abstract class GradientEffectBase<TComponent> where TComponent : Component
    {
        /// <summary>
        /// Applies a gradient effect to the specified component using the provided gradient and direction.
        /// </summary>
        /// <param name="component">The component to apply the gradient to.</param>
        /// <param name="gradient">The gradient containing color keys to use for the effect.</param>
        /// <param name="direction">The direction of the gradient effect.</param>
        /// <remarks>
        /// The gradient must contain at least one color key. If the gradient has multiple color keys,
        /// only the first and last colors will be used for the gradient effect.
        /// </remarks>
        public void ApplyGradient(
            [NotNull] TComponent component,
            [NotNull] Gradient gradient,
            GradientDirection direction)
        {
            if (gradient.colorKeys.Length < 1)
            {
                Debug.LogError("[GradientEffectBase::ApplyGradient] Invalid gradient provided." +
                               " Ensure it has at least one color key.");
                return;
            }

            var startColor = gradient.colorKeys[0].color;
            var endColor = gradient.colorKeys[^1].color;

            ApplyGradient(component, startColor, endColor, direction);
        }

        protected abstract void ApplyGradient(
            TComponent component,
            Color startColor,
            Color endColor,
            GradientDirection direction);

        public abstract void ClearGradient(TComponent component);
    }
}