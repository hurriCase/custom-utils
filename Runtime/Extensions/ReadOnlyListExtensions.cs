using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ZLinq;
using Random = UnityEngine.Random;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    [PublicAPI]
    public static class ReadOnlyListExtensions
    {
        /// <summary>
        /// Finds the range of indices containing non-zero values in the list.
        /// </summary>
        /// <param name="data">The list of integers to analyze.</param>
        /// <returns>A tuple containing the start and end indices of the non-zero range, or null if all values are zero.</returns>
        public static (int startIndex, int endIndex)? GetNonZeroRange(this IReadOnlyList<int> data)
        {
            using var nonZeroIndices = data
                .Select(static (value, index) => (value, index))
                .Where(static tuple => tuple.value > 0)
                .Select(static tuple => tuple.index)
                .ToArrayPool();

            if (nonZeroIndices.Size == 0)
                return null;

            var zeroIndicesSpan = nonZeroIndices.Span;
            return (zeroIndicesSpan[0], zeroIndicesSpan[^1]);
        }

        /// <summary>
        /// Selects <paramref name="count"/> random elements from <paramref name="source"/> and adds them to <paramref name="result"/>.
        /// Uses Fisher-Yates shuffle on stack-allocated indices to avoid heap allocation.
        /// </summary>
        /// <param name="source">The source list to select from.</param>
        /// <param name="count">The number of random elements to select.</param>
        /// <param name="result">The collection to populate with selected elements.</param>
        /// <typeparam name="TValue">The type of elements in the list.</typeparam>
        public static void GetRandom<TValue>(this IReadOnlyList<TValue> source, int count, ICollection<TValue> result)
        {
            Span<int> indices = stackalloc int[source.Count];
            for (var i = 0; i < indices.Length; i++)
                indices[i] = i;

            for (var i = 0; i < count && i < indices.Length; i++)
            {
                var randomIndex = Random.Range(i, indices.Length);
                (indices[i], indices[randomIndex]) = (indices[randomIndex], indices[i]);
                result.Add(source[indices[i]]);
            }
        }
    }
}