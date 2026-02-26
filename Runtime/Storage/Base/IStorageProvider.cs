using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Base
{
    /// <summary>
    /// Defines a storage backend for persisting and retrieving data by key.
    /// </summary>
    [PublicAPI]
    public interface IStorageProvider
    {
        /// <summary>Saves data under the specified key.</summary>
        /// <returns>True if successful, false on error.</returns>
        UniTask<bool> TrySaveAsync<T>(string key, T data, CancellationToken cancellationToken = default);

        /// <summary>Loads data of type <typeparamref name="T"/> for the specified key.</summary>
        /// <returns>The loaded value, or default if not found.</returns>
        UniTask<T> LoadAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>Checks whether the specified key exists in storage.</summary>
        UniTask<bool> HasKeyAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>Deletes the value associated with the specified key.</summary>
        /// <returns>True if successful, false on error.</returns>
        UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>Deletes all stored data. This operation cannot be undone.</summary>
        /// <returns>True if successful, false if unsupported or failed.</returns>
        UniTask<bool> TryDeleteAllAsync(CancellationToken cancellationToken = default);
    }
}