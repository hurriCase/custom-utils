#if ZSTRING_INSTALLED
using Cysharp.Text;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for formatting time values.
    /// </summary>
    [PublicAPI]
    public static class TimeExtensions
    {
        /// <summary>
        /// Converts a total seconds value to a "mm:ss" formatted string.
        /// </summary>
        /// <param name="totalSeconds">The total number of seconds to format.</param>
        /// <returns>A string representing the time in "mm:ss" format.</returns>
        public static string ToTimeFormat(this int totalSeconds)
        {
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;

            var stringBuilder = ZString.CreateStringBuilder();
            try
            {
                AppendTwoDigits(ref stringBuilder, minutes);
                stringBuilder.Append(':');
                AppendTwoDigits(ref stringBuilder, seconds);
                return stringBuilder.ToString();
            }
            finally
            {
                stringBuilder.Dispose();
            }
        }

        private static void AppendTwoDigits(ref Utf16ValueStringBuilder stringBuilder, int value)
        {
            if (value < 10)
                stringBuilder.Append('0');
            stringBuilder.Append(value);
        }
    }
}
#endif