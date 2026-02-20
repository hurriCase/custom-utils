using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Task"/>.
    /// </summary>
    [PublicAPI]
    public static class TaskExtensions
    {
        /// <summary>
        /// Converts a Task to UniTask with external cancellation token support.
        /// </summary>
        /// <typeparam name="T">The type of the task result.</typeparam>
        /// <param name="task">The Task to convert.</param>
        /// <param name="token">The cancellation token to attach.</param>
        /// <returns>A UniTask that can be canceled via the provided token.</returns>
        public static UniTask<T> AsUniTask<T>(this Task<T> task, CancellationToken token) =>
            task.AsUniTask().AttachExternalCancellation(token);

        /// <summary>
        /// Converts a Task to UniTask with external cancellation token support.
        /// </summary>
        /// <param name="task">The Task to convert.</param>
        /// <param name="token">The cancellation token to attach.</param>
        /// <returns>A UniTask that can be canceled via the provided token.</returns>
        public static UniTask AsUniTask(this Task task, CancellationToken token) =>
            task.AsUniTask().AttachExternalCancellation(token);
    }
}