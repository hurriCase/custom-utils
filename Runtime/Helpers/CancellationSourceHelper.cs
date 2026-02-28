using System;
using System.Threading;
using UnityEngine;

// ReSharper disable MemberCanBeInternal
namespace CustomUtils.Runtime.Helpers
{
    /// <summary>
    /// Provides helper methods for managing CancellationTokenSource instances.
    /// </summary>
    public static class CancellationSourceHelper
    {
        /// <summary>
        /// Cancels and disposes an existing CancellationTokenSource, then creates a new instance.
        /// </summary>
        /// <param name="cancellationTokenSource">The CancellationTokenSource reference to refresh.</param>
        /// <remarks>
        /// This method first cancels and disposes the existing token source if it exists,
        /// then creates a new CancellationTokenSource instance and assigns it to the same reference.
        /// This is useful for refreshing a token source before starting a new operation.
        /// </remarks>
        public static void SetNewCancellationTokenSource(ref CancellationTokenSource cancellationTokenSource)
        {
            CancelAndDisposeCancellationTokenSource(ref cancellationTokenSource);

            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Safely cancels and disposes a CancellationTokenSource, then sets it to null.
        /// </summary>
        /// <param name="cancellationTokenSource">The CancellationTokenSource reference to cancel and dispose.</param>
        /// <remarks>
        /// This method handles the complete cleanup of a CancellationTokenSource:
        /// 1. Cancels the token if it's not already cancelled
        /// 2. Disposes the token source to release resources
        /// 3. Sets the reference to null to prevent reuse
        /// </remarks>
        public static void CancelAndDisposeCancellationTokenSource(ref CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null)
                return;

            try
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                    cancellationTokenSource.Cancel();

                cancellationTokenSource.Dispose();
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Debug.LogError("[CancellationSourceHelper::CancelAndDisposeCancellationTokenSource] " +
                               $"Error while cancelling token: {ex.Message}");
            }

            cancellationTokenSource = null;
        }
    }
}