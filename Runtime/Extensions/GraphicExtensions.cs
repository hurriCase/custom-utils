using JetBrains.Annotations;
using UnityEngine.UI;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Graphic"/>.
    /// </summary>
    [PublicAPI]
    public static class GraphicExtensions
    {
        /// <summary>
        /// Sets the alpha value of the graphic's color.
        /// </summary>
        /// <param name="graphic">The graphic to modify.</param>
        /// <param name="alpha">The alpha value to set, where 0 is fully transparent and 1 is fully opaque.</param>
        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }
}