using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="CanvasGroup"/>.
    /// </summary>
    [PublicAPI]
    public static class CanvasExtensions
    {
        /// <summary>
        /// Hides a canvas group by setting it to transparent and disabling interaction.
        /// </summary>
        /// <param name="canvasGroup">The canvas group to hide.</param>
        /// <remarks>
        /// Sets alpha to 0 and disables both interactable and blocksRaycasts properties.
        /// </remarks>
        public static void Hide(this CanvasGroup canvasGroup)
        {
            canvasGroup.SetVisible(false);
        }

        /// <summary>
        /// Shows a canvas group by making it fully visible and enabling interaction.
        /// </summary>
        /// <param name="canvasGroup">The canvas group to show.</param>
        /// <remarks>
        /// Sets alpha to 1 and enables both interactable and blocksRaycasts properties.
        /// </remarks>
        public static void Show(this CanvasGroup canvasGroup)
        {
            canvasGroup.SetVisible(true);
        }

        /// <summary>
        /// Sets the visibility state of a canvas group.
        /// </summary>
        /// <param name="canvasGroup">The canvas group to modify.</param>
        /// <param name="isVisible">True to show the canvas group, false to hide it.</param>
        /// <remarks>
        /// When true, sets alpha to 1 and enables interaction. When false, sets alpha to 0 and disables interaction.
        /// </remarks>
        public static void SetVisible(this CanvasGroup canvasGroup, bool isVisible)
        {
            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }
    }
}