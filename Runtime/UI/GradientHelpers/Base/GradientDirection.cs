using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.GradientHelpers.Base
{
    /// <summary>
    /// Defines the direction in which a gradient effect should be applied to UI components.
    /// </summary>
    [PublicAPI]
    public enum GradientDirection
    {
        /// <summary>
        /// No gradient effect is applied. The component retains its original appearance.
        /// </summary>
        None = 0,

        /// <summary>
        /// Gradient flows from left to right across the component.
        /// </summary>
        LeftToRight = 1,

        /// <summary>
        /// Gradient flows from top to bottom across the component.
        /// </summary>
        TopToBottom = 2,

        /// <summary>
        /// Gradient flows from right to left across the component.
        /// </summary>
        RightToLeft = 3,

        /// <summary>
        /// Gradient flows from bottom to top across the component.
        /// </summary>
        BottomToTop = 4
    }
}