using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Theme
{
    /// <summary>
    /// Defines the different types of colors that can be applied to UI elements in the theme system.
    /// </summary>
    [PublicAPI]
    public enum ColorType
    {
        /// <summary>
        /// No color type specified or default state.
        /// </summary>
        None = 0,

        /// <summary>
        /// Solid color that applies a single uniform color to the UI element.
        /// </summary>
        Solid = 1,

        /// <summary>
        /// Gradient color. Text and simple gradients use vertex colors, every other gradient uses the gradient shader.
        /// </summary>
        Gradient = 2
    }
}