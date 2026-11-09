using System;
using System.Threading;
using System.Threading.Tasks;
using CustomUtils.Runtime.ResponseTypes;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

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

        /// <summary>
        /// Executes the task and wraps the result in a <see cref="Result{TResult}"/>, suppressing any exceptions.
        /// Logs the exception on failure.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        /// <param name="task">The task to execute.</param>
        /// <returns>
        /// <see cref="Result{TResult}.Valid"/> with the result on success;
        /// <see cref="Result{TResult}.Invalid"/> on failure.
        /// </returns>
        public static UniTask<Result<TResult>> SuppressAsync<TResult>(
            this Task<TResult> task)
            => ExecuteSuppressedAsync(task.AsUniTask());

        /// <summary>
        /// Executes the task and wraps the result in a <see cref="Result{TResult}"/>, suppressing any exceptions.
        /// Cancels waiting when <paramref name="cancellationToken"/> is triggered.
        /// Logs the exception on failure.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        /// <param name="task">The task to execute.</param>
        /// <param name="cancellationToken">Token to cancel awaiting the task on the client side.</param>
        /// <returns>
        /// <see cref="Result{TResult}.Valid"/> with the result on success;
        /// <see cref="Result{TResult}.Invalid"/> on failure or cancellation.
        /// </returns>
        public static UniTask<Result<TResult>> SuppressAsync<TResult>(
            this Task<TResult> task,
            CancellationToken cancellationToken)
            => ExecuteSuppressedAsync(task.AsUniTask(cancellationToken));

        /// <summary>
        /// Executes the task, suppressing any exceptions.
        /// Logs the exception on failure.
        /// </summary>
        /// <param name="task">The task to execute.</param>
        /// <returns><see langword="true"/> on success; <see langword="false"/> on failure.</returns>
        public static UniTask<bool> SuppressAsync(
            this Task task)
            => ExecuteSuppressedAsync(task.AsUniTask());

        /// <summary>
        /// Executes the task, suppressing any exceptions.
        /// Cancels waiting when <paramref name="cancellationToken"/> is triggered.
        /// Logs the exception on failure.
        /// </summary>
        /// <param name="task">The task to execute.</param>
        /// <param name="cancellationToken">Token to cancel awaiting the task on the client side.</param>
        /// <returns><see langword="true"/> on success; <see langword="false"/> on failure or cancellation.</returns>
        public static UniTask<bool> SuppressAsync(
            this Task task,
            CancellationToken cancellationToken)
            => ExecuteSuppressedAsync(task.AsUniTask(cancellationToken));

        private static async UniTask<Result<TResult>> ExecuteSuppressedAsync<TResult>(UniTask<TResult> task)
        {
            try
            {
                return Result<TResult>.Valid(await task);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Debug.LogException(exception);
                return Result<TResult>.Invalid();
            }
        }

        private static async UniTask<bool> ExecuteSuppressedAsync(UniTask task)
        {
            try
            {
                await task;
                return true;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Debug.LogException(exception);
                return false;
            }
        }
    }
}