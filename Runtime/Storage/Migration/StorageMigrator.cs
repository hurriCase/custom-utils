using System;
using System.Collections.Generic;
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
        /// Copies the specified keys from one provider to another.
        /// </summary>
        /// <param name="fromProvider">Source provider to read from.</param>
        /// <param name="toProvider">Destination provider to write to.</param>
        /// <param name="keys">Keys to migrate.</param>
        /// <param name="deleteFromSource">If true, deletes each key from the source after a successful migration.</param>
        /// <returns>The number of successfully migrated keys.</returns>
        public static async UniTask<int> MigrateAsync(
            IStorageProvider fromProvider,
            IStorageProvider toProvider,
            IReadOnlyList<string> keys,
            bool deleteFromSource = false)
        {
            var migratedCount = 0;

            foreach (var key in keys)
            {
                try
                {
                    if (!await fromProvider.HasKeyAsync(key))
                    {
                        Logger.LogWarning($"[StorageMigrator::MigrateAsync] Key '{key}' not found in source provider");
                        continue;
                    }

                    var data = await fromProvider.LoadAsync<object>(key);
                    if (data == null)
                        continue;

                    var success = await toProvider.TrySaveAsync(key, data);
                    if (!success)
                        continue;

                    if (deleteFromSource)
                        await fromProvider.TryDeleteKeyAsync(key);

                    migratedCount++;
                    Logger.LogWarning($"[StorageMigrator::MigrateAsync] Migrated key: {key}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[StorageMigrator::MigrateAsync] Failed to migrate key '{key}': {ex.Message}");
                }
            }

            Logger.Log("[StorageMigrator::MigrateAsync] " +
                       $"Migration complete: {migratedCount}/{keys.Count} keys migrated");

            return migratedCount;
        }
    }
}