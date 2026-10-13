using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Color"/>.
    /// </summary>
    [PublicAPI]
    public static class ColorExtensions
    {
        /// <summary>
        /// Checks whether two colors are the same at 8 bits per channel, the precision colors are shown and stored in.
        /// </summary>
        /// <param name="first">The first color</param>
        /// <param name="second">The color to compare with</param>
        /// <returns>True if every channel matches after rounding to 8 bits</returns>
        public static bool Approximately(this Color first, Color second)
        {
            var firstColor = (Color32)first;
            var secondColor = (Color32)second;
            return firstColor.r == secondColor.r
                   && firstColor.g == secondColor.g
                   && firstColor.b == secondColor.b
                   && firstColor.a == secondColor.a;
        }
    }
}
