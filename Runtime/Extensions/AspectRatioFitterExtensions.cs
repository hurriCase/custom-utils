using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="AspectRatioFitter"/>.
    /// </summary>
    [PublicAPI]
    public static class AspectRatioFitterExtensions
    {
        /// <summary>
        /// Creates a copy with WidthControlsHeight mode.
        /// </summary>
        /// <param name="aspectRatioFitter">Source AspectRatioFitter to copy.</param>
        /// <param name="spacing">Aspect ratio value.</param>
        /// <param name="container">Parent container.</param>
        /// <returns>New AspectRatioFitter with WidthControlsHeight mode.</returns>
        public static AspectRatioFitter CreateWidthSpacing(
            this AspectRatioFitter aspectRatioFitter,
            float spacing,
            RectTransform container) =>
            aspectRatioFitter.CreateSpacing(spacing, container, AspectRatioFitter.AspectMode.WidthControlsHeight);

        /// <summary>
        /// Creates a copy with HeightControlsWidth mode.
        /// </summary>
        /// <param name="aspectRatioFitter">Source AspectRatioFitter to copy.</param>
        /// <param name="spacing">Aspect ratio value.</param>
        /// <param name="container">Parent container.</param>
        /// <returns>New AspectRatioFitter with HeightControlsWidth mode.</returns>
        public static AspectRatioFitter CreateHeightSpacing(
            this AspectRatioFitter aspectRatioFitter,
            float spacing,
            RectTransform container) =>
            aspectRatioFitter.CreateSpacing(spacing, container);

        /// <summary>
        /// Creates a copy with specified aspect mode.
        /// </summary>
        /// <param name="aspectRatioFitter">Source AspectRatioFitter to copy.</param>
        /// <param name="spacing">Aspect ratio value.</param>
        /// <param name="container">Parent container.</param>
        /// <param name="aspectMode">Aspect mode. Defaults to HeightControlsWidth.</param>
        /// <returns>New AspectRatioFitter with specified configuration.</returns>
        public static AspectRatioFitter CreateSpacing(
            this AspectRatioFitter aspectRatioFitter,
            float spacing,
            RectTransform container,
            AspectRatioFitter.AspectMode aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth)
        {
            var createdSpacing = Object.Instantiate(aspectRatioFitter, container);
            createdSpacing.aspectRatio = spacing;
            createdSpacing.aspectMode = aspectMode;
            return createdSpacing;
        }
    }
}