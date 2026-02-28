using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Type"/>.
    /// </summary>
    [PublicAPI]
    public static class TypeExtensions
    {
        /// <summary>
        /// Attempts to extract the enum type from a generic type or array.
        /// </summary>
        /// <param name="type">The type to extract the enum type from.</param>
        /// <param name="enumType">The extracted enum type if found; otherwise, null.</param>
        /// <returns>True if the enum type was successfully extracted; otherwise, false.</returns>
        public static bool TryGetEnumType(this Type type, out Type enumType)
        {
            enumType = null;
            if (type.IsArray)
                type = type.GetElementType();

            if (type is null)
                return false;

            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length <= 0)
                return false;

            enumType = genericArgs[0];
            return true;
        }

        /// <summary>
        /// Gets the distinct enum member names, excluding duplicate values.
        /// </summary>
        /// <param name="enumType">The enum type to get distinct names from.</param>
        /// <returns>An array of distinct enum member names, or an empty array if the type is not an enum.</returns>
        public static string[] GetDistinctEnumNames(this Type enumType)
        {
            if (!typeof(Enum).IsAssignableFrom(enumType))
                return Array.Empty<string>();

            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType);
            var distinctNames = new List<string>();
            var seenValues = new HashSet<int>();

            for (var i = 0; i < names.Length; i++)
            {
                var intValue = Convert.ToInt32(values.GetValue(i));
                if (seenValues.Add(intValue))
                    distinctNames.Add(names[i]);
            }

            return distinctNames.ToArray();
        }
    }
}