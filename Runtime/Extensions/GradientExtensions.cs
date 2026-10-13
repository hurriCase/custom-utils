using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Gradient"/>.
    /// </summary>
    [PublicAPI]
    public static class GradientExtensions
    {
        private const float KeyTimeTolerance = 0.002f;

        /// <summary>
        /// Checks whether two gradients look the same: key times match within a small tolerance,
        /// and key colors and alphas match at 8 bits per channel.
        /// </summary>
        /// <param name="first">The first gradient</param>
        /// <param name="second">The gradient to compare with</param>
        /// <returns>True if both gradients have the same keys at display precision</returns>
        public static bool Approximately(this Gradient first, Gradient second)
        {
            var firstColorKeys = first.colorKeys;
            var secondColorKeys = second.colorKeys;
            var firstAlphaKeys = first.alphaKeys;
            var secondAlphaKeys = second.alphaKeys;

            if (firstColorKeys.Length != secondColorKeys.Length || firstAlphaKeys.Length != secondAlphaKeys.Length)
                return false;

            for (var i = 0; i < firstColorKeys.Length; i++)
            {
                var timeDifference = Mathf.Abs(firstColorKeys[i].time - secondColorKeys[i].time);
                var isSameColor = firstColorKeys[i].color.Approximately(secondColorKeys[i].color);

                if (timeDifference > KeyTimeTolerance || !isSameColor)
                    return false;
            }

            for (var i = 0; i < firstAlphaKeys.Length; i++)
            {
                var firstAlpha = (byte)Mathf.RoundToInt(firstAlphaKeys[i].alpha * byte.MaxValue);
                var secondAlpha = (byte)Mathf.RoundToInt(secondAlphaKeys[i].alpha * byte.MaxValue);
                var timeDifference = Mathf.Abs(firstAlphaKeys[i].time - secondAlphaKeys[i].time);

                if (timeDifference > KeyTimeTolerance || firstAlpha != secondAlpha)
                    return false;
            }

            return true;
        }
    }
}
