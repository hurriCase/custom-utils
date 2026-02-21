using System.Runtime.CompilerServices;
using JetBrains.Annotations;

#if ZSTRING_INSTALLED
using Cysharp.Text;
#endif

namespace CustomUtils.Runtime.Formatter
{
    [PublicAPI]
    public static class StringFormatter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T1>(string format, T1 arg1)
        {
#if ZSTRING_INSTALLED
            return ZString.Format(format, arg1);
#else
            return string.Format(format, arg1);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T1, T2>(string format, T1 arg1, T2 arg2)
        {
#if ZSTRING_INSTALLED
            return ZString.Format(format, arg1, arg2);
#else
            return string.Format(format, arg1, arg2);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
#if ZSTRING_INSTALLED
            return ZString.Format(format, arg1, arg2, arg3);
#else
            return string.Format(format, arg1, arg2, arg3);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
#if ZSTRING_INSTALLED
            return ZString.Format(format, arg1, arg2, arg3, arg4);
#else
            return string.Format(format, arg1, arg2, arg3, arg4);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
#if ZSTRING_INSTALLED
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5);
#else
            return string.Format(format, arg1, arg2, arg3, arg4, arg5);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Concat<T1, T2>(T1 arg1, T2 arg2)
        {
#if ZSTRING_INSTALLED
            return ZString.Concat(arg1, arg2);
#else
            return string.Concat(arg1, arg2);
#endif
        }
    }
}