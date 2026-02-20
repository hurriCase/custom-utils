using System.Collections.Generic;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="List{T}"/> collections.
    /// </summary>
    [PublicAPI]
    public static class ListExtensions
    {
        /// <summary>
        /// Gets an element at the specified index, or creates and adds default elements
        /// to the list until the index is accessible, then returns the element at that index.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to extend or retrieve from.</param>
        /// <param name="index">The zero-based index of the element to get or create.</param>
        /// <param name="defaultValue">The default value to use when creating new elements.
        /// If not specified, uses the default value for type T.</param>
        /// <returns>The element at the specified index.</returns>
        public static T GetOrCreate<T>([NotNull] this IList<T> list, int index, T defaultValue = default)
        {
            while (list.Count <= index)
            {
                list.Add(defaultValue);
            }

            return list[index];
        }

        /// <summary>
        /// Gets a random element from the list using Unity's Random number generator.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to get a random element from.</param>
        /// <returns>A randomly selected element from the list.</returns>
        public static T Random<T>([NotNull] this IList<T> list) => list[UnityEngine.Random.Range(0, list.Count)];
    }
}