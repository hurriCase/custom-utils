using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="AsyncOperationHandle{T}"/>.
    /// </summary>
    [PublicAPI]
    public static class AsyncOperationHandleExtensions
    {
        /// <summary>
        /// Awaits the operation, releases the handle, and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the operation result.</typeparam>
        /// <param name="handle">The handle to execute and release.</param>
        /// <returns>The result of the completed operation.</returns>
        /// <exception cref="UnityEngine.AddressableAssets.InvalidKeyException">
        /// Thrown if the operation failed; wraps the original <see cref="AsyncOperationHandle.OperationException"/>.
        /// </exception>
        public static async UniTask<TResult> ExecuteAsyncAndRelease<TResult>(this AsyncOperationHandle<TResult> handle)
        {
            try
            {
                await handle.Task;
                return handle.Status != AsyncOperationStatus.Succeeded ? throw handle.OperationException : handle.Result;
            }
            finally
            {
                Addressables.Release(handle);
            }
        }
    }
}