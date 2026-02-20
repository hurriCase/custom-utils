using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for working with JSON-related tasks.
    /// </summary>
    [PublicAPI]
    public static class JsonExtension
    {
        /// <summary>
        /// Gets the JSON property name for an enum value, using either the JsonPropertyAttribute
        /// value or the enum value's string representation.
        /// </summary>
        /// <param name="enumValue">The enum value to get the JSON property name for.</param>
        public static string GetJsonPropertyName(this Type enumValue)
        {
            var attribute = enumValue
                .GetField(enumValue.ToString())
                ?.GetCustomAttributes(typeof(JsonPropertyAttribute), false)
                .FirstOrDefault() as JsonPropertyAttribute;

            return attribute?.PropertyName ?? enumValue.ToString();
        }
    }
}