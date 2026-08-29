#if ZSTRING_INSTALLED
using Cysharp.Text;

namespace CustomUtils.Runtime.Extensions
{
    internal static class TimeExtensions
    {
        internal static string ToTimeFormat(this int totalSeconds)
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