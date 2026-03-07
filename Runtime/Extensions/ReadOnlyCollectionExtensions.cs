using System.Collections.Generic;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IReadOnlyCollection{T}"/>.
    /// </summary>
    [PublicAPI]
    public static class ReadOnlyCollectionExtensions
    {
        /// <summary>
        /// Returns a random element from <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The collection to select from.</param>
        /// <typeparam name="TItem">The type of elements in the collection.</typeparam>
        /// <returns>A randomly selected element.</returns>
        public static TItem GetRandom<TItem>(this IReadOnlyCollection<TItem> collection)
        {
            var index = Random.Range(0, collection.Count);
            return collection.ElementAt(index);
        }
    }
}