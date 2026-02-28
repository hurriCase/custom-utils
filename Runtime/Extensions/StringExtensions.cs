using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CustomUtils.Runtime.Formatter;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="string"/>.
    /// </summary>
    [PublicAPI]
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a property name to its backing field representation
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The backing field name format</returns>
        public static string ConvertToBackingField(this string propertyName) => $"<{propertyName}>k__BackingField";

        /// <summary>
        /// Converts a camelCase or PascalCase string to a spaced string.
        /// </summary>
        /// <param name="str">The camelCase or PascalCase string to convert.</param>
        /// <returns>A string with spaces between words.</returns>
        public static string ToSpacedWords(this string str) =>
            string.IsNullOrEmpty(str) ? str : Regex.Replace(str, "([a-z])([A-Z])", "$1 $2");

        /// <summary>
        /// Extracts the substring that appears after the first occurrence of the specified character.
        /// </summary>
        /// <param name="str">The input string to search within.</param>
        /// <param name="symbol">The character to search for as a delimiter.</param>
        /// <returns>
        /// The substring after the first occurrence of the specified character.
        /// Returns an empty string if the character is not found or if it's the last character in the string.
        /// </returns>
        public static string GetTextAfter(this string str, char symbol) => str[(str.IndexOf(symbol) + 1)..];

        /// <summary>
        /// Extracts the substring that appears after the first occurrence of the specified delimiter string.
        /// </summary>
        /// <param name="str">The input string to search within.</param>
        /// <param name="delimiter">The string to search for as a delimiter.</param>
        /// <returns>
        /// The substring after the first occurrence of the specified delimiter.
        /// Returns an empty string if the delimiter is not found or if it appears at the end of the string.
        /// </returns>
        public static string GetTextAfter(this string str, string delimiter)
        {
            var index = str.IndexOf(delimiter, StringComparison.Ordinal);
            return index >= 0 ? str[(index + delimiter.Length)..] : string.Empty;
        }

        /// <summary>
        /// Splits a string by the specified delimiter and returns a list of trimmed string elements,
        /// excluding empty or whitespace-only elements.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <param name="delimiter">The character used as a delimiter for splitting.</param>
        /// <returns>A list of trimmed string elements, with empty elements excluded.</returns>
        public static List<string> SplitToListTrimmed(this string str, char delimiter)
            => SplitToListTrimmed(str, delimiter, static element => element);

        /// <summary>
        /// Splits a string by the specified delimiter, trims each element, and converts them to the specified type
        /// using the provided converter function. Empty or whitespace-only elements are excluded.
        /// </summary>
        /// <typeparam name="T">The type to convert each string element to.</typeparam>
        /// <param name="str">The string to split.</param>
        /// <param name="delimiter">The character used as a delimiter for splitting.</param>
        /// <param name="converter">A function that converts a trimmed string element to type T.</param>
        /// <returns>A list of converted elements of type T, with empty elements excluded.</returns>
        public static List<T> SplitToListTrimmed<T>(this string str, char delimiter, Func<string, T> converter)
        {
            if (string.IsNullOrEmpty(str))
                return new List<T>();

            var result = new List<T>();
            var startIndex = 0;

            for (var i = 0; i <= str.Length; i++)
            {
                if (i != str.Length && str[i] != delimiter)
                    continue;

                if (i > startIndex)
                {
                    var element = ExtractTrimmedElement(str, startIndex, i - 1);
                    if (!string.IsNullOrEmpty(str))
                    {
                        var convertedValue = converter(element);
                        result.Add(convertedValue);
                    }
                }

                startIndex = i + 1;
            }

            return result;
        }

        /// <summary>
        /// Extracts a trimmed substring from the specified range within a string,
        /// removing leading and trailing whitespace characters.
        /// </summary>
        /// <param name="str">The source string to extract from.</param>
        /// <param name="start">The starting index (inclusive) of the substring.</param>
        /// <param name="end">The ending index (inclusive) of the substring.</param>
        /// <returns>
        /// A trimmed substring from the specified range, or an empty string if the range
        /// contains only whitespace or if start index is greater than end index.
        /// </returns>
        public static string ExtractTrimmedElement(string str, int start, int end)
        {
            while (start <= end && char.IsWhiteSpace(str[start]))
            {
                start++;
            }

            while (end >= start && char.IsWhiteSpace(str[end]))
            {
                end--;
            }

            if (start > end)
                return string.Empty;

            using var builder = StringBuilderScope.Create();
            builder.Append(str, start, end - start + 1);
            return builder.ToString();
        }

        /// <summary>
        /// Attempts to retrieve the value of an environment variable.
        /// </summary>
        /// <param name="environmentVariableName">The name of the environment variable to retrieve.</param>
        /// <param name="value">When this method returns,
        /// contains the environment variable value if it exists and is valid; otherwise, null.</param>
        /// <param name="environmentVariableTarget">The target scope for the environment variable lookup.</param>
        /// <returns>True if the environment variable exists and has a valid value; otherwise, false.</returns>
        public static bool TryGetValueFromEnvironment(
            this string environmentVariableName,
            out string value,
            EnvironmentVariableTarget environmentVariableTarget = EnvironmentVariableTarget.Machine)
        {
            value = Environment.GetEnvironmentVariable(environmentVariableName, environmentVariableTarget);

            if (!string.IsNullOrEmpty(value))
                return true;

            Debug.LogError("[StringExtensions::TryGetValueFromEnvironment] " +
                           $"Environment variable value for {environmentVariableName} wasn't found");
            return false;
        }

        /// <summary>
        /// Computes a SHA256 hash of the string and returns the first 16 characters as a hexadecimal string.
        /// </summary>
        /// <param name="text">The text to hash.</param>
        /// <returns>A 16-character hexadecimal string representing the truncated SHA256 hash.</returns>
        public static string GetHash(this string text)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hash).Replace("-", "")[..16]; // The first 16 chars should be enough
        }

        /// <summary>
        /// Creates a directory if it does not exist.
        /// </summary>
        /// <param name="path">The path to the directory to create.</param>
        /// <returns>True if the directory was created, false otherwise.</returns>
        public static void CreateDirectoryIfNotExist(this string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}