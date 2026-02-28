using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="string"/> conversions.
    /// </summary>
    [PublicAPI]
    public static class StringConversionExtensions
    {
        private const char CollectionDelimiter = ';';

        /// <summary>
        /// Safely converts a string to an integer, returning 0 if conversion fails.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The converted integer value, or 0 if conversion fails.</returns>
        public static int ToInt(this string value) => int.TryParse(value, out var result) ? result : 0;

        /// <summary>
        /// Safely converts a string to a float, returning 0f if conversion fails.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The converted float value, or 0f if conversion fails.</returns>
        public static float ToFloat(this string value) => float.TryParse(value, out var result) ? result : 0f;

        /// <summary>
        /// Converts a string to a boolean value. Returns true for "true" (case-insensitive) or "1",
        /// and false for all other values including null or whitespace.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>True if the string equals "true" (case-insensitive) or "1"; otherwise, false.</returns>
        public static bool ToBool(this string value) =>
            !string.IsNullOrWhiteSpace(value) &&
            (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1");

        /// <summary>
        /// Converts a delimited string to a list of trimmed strings using semicolon (;) as the delimiter.
        /// Empty or whitespace-only elements are excluded from the result.
        /// </summary>
        /// <param name="value">The delimited string to convert.</param>
        /// <returns>A list of trimmed string elements, or an empty list if the input is null or whitespace.</returns>
        public static List<string> ToStringList(this string value) => value.SplitToListTrimmed(CollectionDelimiter);

        /// <summary>
        /// Converts a delimited string to a list of integers using semicolon (;) as the delimiter.
        /// Each element is parsed as an integer, and empty or whitespace-only elements are excluded.
        /// </summary>
        /// <param name="value">The delimited string to convert.</param>
        /// <returns>A list of integers, or an empty list if the input is null or whitespace.</returns>
        /// <exception cref="FormatException">Thrown if any non-empty element cannot be parsed as an integer.</exception>
        public static List<int> ToIntList(this string value) =>
            string.IsNullOrWhiteSpace(value)
                ? new List<int>()
                : value.SplitToListTrimmed(CollectionDelimiter, static str => int.Parse(str));

        /// <summary>
        /// Converts a delimited string to a list of type T using semicolon (;) as the delimiter
        /// and the provided converter function to transform each element.
        /// Empty or whitespace-only elements are excluded from the result.
        /// </summary>
        /// <typeparam name="T">The type to convert each string element to.</typeparam>
        /// <param name="value">The delimited string to convert.</param>
        /// <param name="converter">A function that converts a trimmed string element to type T.</param>
        /// <returns>A list of converted elements of type T, or an empty list if the input is null or whitespace.</returns>
        /// <exception cref="Exception">May throw exceptions based on the converter function's implementation.</exception>
        public static List<T> ToList<T>(this string value, Func<string, T> converter) =>
            string.IsNullOrWhiteSpace(value)
                ? new List<T>()
                : value.SplitToListTrimmed(CollectionDelimiter, converter);
    }
}