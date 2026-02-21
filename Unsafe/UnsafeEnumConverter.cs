using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace CustomUtils.Unsafe
{
    /// <summary>
    /// Fast enum to int conversion using unsafe code. Zero allocations.
    /// </summary>
    /// <typeparam name="T">Enum type (must be 4-byte enum like int/uint)</typeparam>
    [PublicAPI]
    public static class UnsafeEnumConverter<T> where T : unmanaged, Enum
    {
        /// <summary>
        /// Converts enum to int. No validation performed.
        /// </summary>
        /// <param name="value">Enum value to convert</param>
        /// <returns>Integer representation of the enum</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ToInt32(T value) => *(int*)&value;

        /// <summary>
        /// Converts int to enum. No validation performed.
        /// </summary>
        /// <param name="value">Integer value to convert</param>
        /// <returns>Enum with the specified integer value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T FromInt32(int value) => *(T*)&value;
    }
}