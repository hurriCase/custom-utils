using System;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Migration
{
    /// <summary>
    /// Utility for migrating data between storage providers.
    /// </summary>
    [PublicAPI]
    public static class StorageMigrator
    {
        /// <summary>
        /// Copies one key from one provider to another.
        /// The type is required because serializers like MemoryPack can only read data as its exact type.
        /// </summary>
        /// <typeparam name="TData">The type the value was saved as</typeparam>
        /// <param name="fromProvider">Source provider to read from</param>
        /// <param name="toProvider">Destination provider to write to</param>
        /// <param name="key">Key to migrate</param>
        /// <param name="deleteFromSource">If true, deletes the key from the source after a successful migration</param>
        /// <returns>True if the key was migrated, false if it wasn't found or couldn't be loaded or saved</returns>
        public static async UniTask<bool> MigrateAsync<TData>(
            IStorageProvider fromProvider,
            IStorageProvider toProvider,
            string key,
            bool deleteFromSource = false)
        {
            try
            {
                var loadResult = await fromProvider.TryLoadAsync<TData>(key);

                if (loadResult.Status == LoadStatus.NotFound)
                {
                    Logger.LogWarning($"[{nameof(StorageMigrator)}::{nameof(MigrateAsync)}] " +
                                      $"Key '{key}' not found in source provider");
                    return false;
                }

                if (loadResult.Status == LoadStatus.Failed || !await toProvider.TrySaveAsync(key, loadResult.Data))
                    return false;

                if (deleteFromSource)
                    await fromProvider.TryDeleteKeyAsync(key);

                Logger.Log($"[{nameof(StorageMigrator)}::{nameof(MigrateAsync)}] Migrated key '{key}'");
                return true;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Logger.LogException(exception);
                Logger.LogError($"[{nameof(StorageMigrator)}::{nameof(MigrateAsync)}] " +
                                $"Failed to migrate key '{key}': {exception.Message}");
                return false;
            }
        }
    }
}
