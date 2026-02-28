using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for mathematical operations.
    /// </summary>
    [PublicAPI]
    public static class MathExtensions
    {
        private const float LowerThreshold = 0.1f;
        private const float UpperThreshold = 100000f;

        private const float Max16BitValue = 65535f;

        private static readonly Vector2 _decodeDot = new(1.0f, 1f / Max16BitValue);

        /// <summary>
        /// Determines if the given float value is within a reasonable range.
        /// </summary>
        /// <param name="value">The float value to evaluate.</param>
        /// <returns>True if the value is greater than the lower threshold and less than the upper threshold;
        /// otherwise, false.</returns>
        public static bool IsReasonable(this float value) => value is > LowerThreshold and < UpperThreshold;

        /// <summary>
        /// Clamps all components of the Vector4 instance to ensure they are non-negative.
        /// </summary>
        /// <param name="vector4">The Vector4 instance to be modified.</param>
        /// <returns>A new Vector4 instance with all components clamped to zero or higher.</returns>
        public static Vector4 ClampToPositive(this Vector4 vector4)
        {
            vector4.x = Mathf.Max(vector4.x, 0);
            vector4.y = Mathf.Max(vector4.y, 0);
            vector4.z = Mathf.Max(vector4.z, 0);
            vector4.w = Mathf.Max(vector4.w, 0);
            return vector4;
        }

        /// <summary>
        /// Finds the minimum value from multiple float values without allocating memory.
        /// </summary>
        public static float MinOf(float value1, float value2, float value3, float value4, float value5)
            => Mathf.Min(Mathf.Min(Mathf.Min(Mathf.Min(value1, value2), value3), value4), value5);

        /// <summary>
        /// Determines whether the specified integer is a power of two.
        /// </summary>
        /// <param name="x">The integer value to check.</param>
        /// <returns>
        /// <c>true</c> if the specified integer is a power of two (2, 4, 8, 16, etc.);
        /// <c>false</c> if the integer is zero or not a power of two.
        /// </returns>
        public static bool IsPowerOfTwo(this int x) => x != 0 && (x & (x - 1)) == 0;

        /// <summary>
        /// Calculates the scale factor for Vector4 values that need to fit within rectangular bounds.
        /// Uses a constraint pattern where opposing pairs must fit within width/height.
        /// </summary>
        /// <param name="rect">The rectangle bounds</param>
        /// <param name="values">Vector4 values to scale (x,y pair and z,w pair for width; x,w pair and z,y pair for height)</param>
        /// <returns>Scale factor to apply (clamped to maximum of 1.0)</returns>
        public static float CalculateScaleFactorForBounds(this Rect rect, Vector4 values) =>
            MinOf(
                rect.width / (values.x + values.y),  // First width constraint
                rect.width / (values.z + values.w),  // Second width constraint
                rect.height / (values.x + values.w), // First height constraint
                rect.height / (values.z + values.y), // Second height constraint
                1f                                   // Maximum scale
            );

        /// <summary>
        /// Encodes two floating point numbers as a single 16-bit normalized value.
        /// </summary>
        /// <param name="a">The first floating point number to encode.</param>
        /// <param name="b">The second floating point number to encode.</param>
        /// <returns>A float value representing the encoded values of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static float PackAs16BitWith(this float a, float b)
        {
            var encodedValues = new Vector2(
                Mathf.Floor(a * (Max16BitValue - 1)) / Max16BitValue,
                Mathf.Floor(b * (Max16BitValue - 1)) / Max16BitValue
            );

            return Vector2.Dot(encodedValues, _decodeDot);
        }

        /// <summary>
        /// Converts an angle in degrees to its equivalent in radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees to convert.</param>
        /// <returns>The equivalent angle in radians.</returns>
        public static float ToRadians(this float degrees) => degrees * Mathf.Deg2Rad;

        /// <summary>
        /// Calculates the directional vector from a given angle in radians.
        /// </summary>
        /// <param name="angleInRadians">The angle in radians for which the direction vector is calculated.</param>
        /// <returns>A 2D vector representing the direction corresponding to the given angle.</returns>
        public static Vector2 GetDirectionFromAngle(this float angleInRadians)
            => new(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        /// <summary>
        /// Calculates the initial upward velocity required to reach a specified vertical displacement.
        /// </summary>
        /// <param name="displacement">The desired vertical displacement in meters.</param>
        /// <returns>The initial velocity needed to achieve the displacement.</returns>
        public static float GetInitialVelocity(this float displacement)
            => Mathf.Sqrt(displacement * -2f * Physics.gravity.y);

        /// <summary>
        /// Converts a percentage value to a damage multiplier.
        /// For example, 30% becomes 0.7 multiplier.
        /// </summary>
        /// <param name="percent">The percentage to convert.</param>
        /// <returns>A multiplier representing the remaining percentage as a fraction of 1.</returns>
        public static float ToMultiplier(this float percent) => (100f - percent) / 100f;
    }
}