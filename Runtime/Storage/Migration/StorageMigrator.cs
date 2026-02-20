using System;
using System.Collections.Generic;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Migration
{
    /// <summary>
    /// Simple storage migration between providers
    /// </summary>
    [PublicAPI]
    public static class StorageMigrator
    {
        /// <summary>
        /// Migrate specific keys from one provider to another
        /// </summary>
        /// <param name="fromProvider">Source provider</param>
        /// <param name="toProvider">Destination provider</param>
        /// <param name="keys">Keys to migrate</param>
        /// <param name="deleteFromSource">Delete from source after migration</param>
        /// <returns>Number of successfully migrated keys</returns>
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
                    if (await fromProvider.HasKeyAsync(key) is false)
                    {
                        Debug.LogWarning("[StorageMigrator::MigrateAsync] Key '{key}' not found in source provider");
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
                    Debug.LogWarning($"[StorageMigrator::MigrateAsync] Migrated key: {key}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[StorageMigrator::MigrateAsync] Failed to migrate key '{key}': {ex.Message}");
                }
            }

            Debug.Log("[StorageMigrator::MigrateAsync] " +
                      $"Migration complete: {migratedCount}/{keys.Count} keys migrated");

            return migratedCount;
        }
    }
}