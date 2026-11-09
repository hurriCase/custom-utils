using System;
using System.Threading;
using CustomUtils.Runtime.ResponseTypes;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="UniTask"/>.
    /// </summary>
    [PublicAPI]
    public static class UniTaskExtensions
    {
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
            this UniTask<TResult> task)
            => ExecuteSuppressedAsync(task);

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
            this UniTask<TResult> task,
            CancellationToken cancellationToken)
            => ExecuteSuppressedAsync(task.AttachExternalCancellation(cancellationToken));

        /// <summary>
        /// Executes the task, suppressing any exceptions.
        /// Logs the exception on failure.
        /// </summary>
        /// <param name="task">The task to execute.</param>
        /// <returns><see langword="true"/> on success; <see langword="false"/> on failure.</returns>
        public static UniTask<bool> SuppressAsync(
            this UniTask task)
            => ExecuteSuppressedAsync(task);

        /// <summary>
        /// Executes the task, suppressing any exceptions.
        /// Cancels waiting when <paramref name="cancellationToken"/> is triggered.
        /// Logs the exception on failure.
        /// </summary>
        /// <param name="task">The task to execute.</param>
        /// <param name="cancellationToken">Token to cancel awaiting the task on the client side.</param>
        /// <returns><see langword="true"/> on success; <see langword="false"/> on failure or cancellation.</returns>
        public static UniTask<bool> SuppressAsync(
            this UniTask task,
            CancellationToken cancellationToken)
            => ExecuteSuppressedAsync(task.AttachExternalCancellation(cancellationToken));

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