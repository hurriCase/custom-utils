using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace CustomUtils.Unmanaged
{
    /// <summary>
    /// Provides high-performance extension methods for <see cref="Enum"/> types.
    /// </summary>
    [PublicAPI]
    public static class EnumExtensions
    {
        /// <summary>
        /// Checks whether the specified flag is set, without boxing or virtual dispatch.
        /// </summary>
        /// <typeparam name="TEnum">The enum type. Must be backed by <see cref="int"/>.</typeparam>
        /// <param name="value">The enum value to check.</param>
        /// <param name="flag">The flag to test for.</param>
        /// <returns><c>true</c> if all bits in <paramref name="flag"/> are set in <paramref name="value"/>.</returns>
        public static bool HasFlagNoAlloc<TEnum>(this TEnum value, TEnum flag) where TEnum : struct, Enum
        {
            var intValue = Unsafe.As<TEnum, int>(ref value);
            var intFlag = Unsafe.As<TEnum, int>(ref flag);
            return (intValue & intFlag) == intFlag;
        }
    }
}