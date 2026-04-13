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
        /// Gradient color that applies a gradient effect to graphic UI elements
        /// using <see cref="CustomUtils.Runtime.UI.GradientHelpers.GraphicGradient.VertexGradientEffect"/>
        /// by modifying vertex colors.
        /// </summary>
        VertexGraphicGradient = 2,

        /// <summary>
        /// Gradient color specifically designed for text UI elements using TextMeshPro.
        /// </summary>
        TextGradient = 3,

        /// <summary>
        /// Gradient color that applies a gradient effect to graphic UI elements using shaders
        /// and support more interesting variations like diamond, multi gradients.
        /// </summary>
        ShaderGraphicGradient = 4
    }
}