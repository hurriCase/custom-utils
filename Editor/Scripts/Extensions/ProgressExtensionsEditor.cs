using System;
using JetBrains.Annotations;

namespace CustomUtils.Editor.Scripts.Extensions
{
    /// <summary>
    /// Provides Editor time extension methods for <see cref="IProgress{T}"/>.
    /// </summary>
    [PublicAPI]
    public static class ProgressExtensionsEditor
    {
        /// <summary>
        /// Updates the progress based on the number of processed items relative to the total number of items.
        /// </summary>
        /// <param name="progress">The progress indicator to report updates to.</param>
        /// <param name="processedCount">The number of items that have been processed. This is incremented by the method.</param>
        /// <param name="totalCount">The total number of items to process.</param>
        public static void UpdateProgress(this IProgress<float> progress, ref int processedCount, int totalCount)
        {
            processedCount++;
            progress.Report((float)processedCount / totalCount);
        }
    }
}