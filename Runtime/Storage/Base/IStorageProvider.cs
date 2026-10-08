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
        UniTask<bool> TrySaveAsync<T>(string key, T data, bool isForce = false);

        /// <summary>Loads data of type <typeparamref name="T"/> for the specified key.</summary>
        /// <returns>
        /// <see cref="LoadStatus.Loaded"/> with the data, <see cref="LoadStatus.NotFound"/> if nothing is stored,
        /// or <see cref="LoadStatus.Failed"/> if stored data couldn't be read. Only throws on cancellation.
        /// </returns>
        UniTask<LoadResult<T>> TryLoadAsync<T>(string key, CancellationToken token = default);

        /// <summary>Checks whether the specified key exists in storage.</summary>
        UniTask<bool> HasKeyAsync(string key, CancellationToken token = default);

        /// <summary>Deletes the value associated with the specified key.</summary>
        /// <returns>True if successful, false on error.</returns>
        UniTask<bool> TryDeleteKeyAsync(string key, CancellationToken token = default);

        /// <summary>Deletes all stored data. This operation cannot be undone.</summary>
        /// <returns>True if successful, false if unsupported or failed.</returns>
        UniTask<bool> TryDeleteAllAsync(CancellationToken token = default);
    }
}