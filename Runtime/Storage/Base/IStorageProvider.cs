using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Base
{
    /// <summary>
    /// Provides storage operations with automatic caching and serialization.
    /// </summary>
    [PublicAPI]
    public interface IStorageProvider
    {
        /// <summary>
        /// Saves data to storage with the specified key.
        /// Data is automatically cached.
        /// </summary>
        /// <typeparam name="T">The type of data to save.</typeparam>
        /// <param name="key">Unique key for the data.</param>
        /// <param name="data">Data to save.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successful, false on error.</returns>
        UniTask<bool> TrySaveAsync<T>(string key, T data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads data from storage with the specified key.
        /// Data is automatically cached for faster subsequent access.
        /// </summary>
        /// <typeparam name="T">The type of data to load.</typeparam>
        /// <param name="key">Unique key for the data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The loaded data, or default value if not found.</returns>
        UniTask<T> LoadAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a key exists in storage.
        /// Checks cache first for better performance.
        /// </summary>
        /// <param name="key">Key to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if key exists, false otherwise.</returns>
        UniTask<bool> HasKeyAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes data with the specified key from storage.
        /// Also removes from cache.
        /// </summary>
        /// <param name="key">Key of data to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successful, false on error.</returns>
        UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to delete ALL data from storage.
        /// This operation cannot be undone!
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successful, false if not supported or failed.</returns>
        UniTask<bool> TryDeleteAllAsync(CancellationToken cancellationToken = default);
    }
}