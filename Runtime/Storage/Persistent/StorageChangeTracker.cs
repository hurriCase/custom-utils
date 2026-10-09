using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace CustomUtils.Runtime.Storage.Persistent
{
    internal static class StorageChangeTracker
    {
        private static readonly HashSet<IStorageEntry> _changedEntries = new();
        private static StorageLifecycle _lifecycle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _changedEntries.Clear();
            _lifecycle = null;
        }

        internal static void MarkChanged(IStorageEntry entry)
        {
            _changedEntries.Add(entry);

            if (!_lifecycle && Application.isPlaying)
                CreateLifecycle();
        }

        internal static void MarkFlushed(IStorageEntry entry)
        {
            _changedEntries.Remove(entry);
        }

        internal static void FlushAll()
        {
            using var _ = ListPool<IStorageEntry>.Get(out var entries);
            entries.AddRange(_changedEntries);

            foreach (var entry in entries)
                entry.Flush();
        }

        private static void CreateLifecycle()
        {
            var gameObject = new GameObject(nameof(StorageLifecycle)) { hideFlags = HideFlags.HideInHierarchy };
            Object.DontDestroyOnLoad(gameObject);
            _lifecycle = gameObject.AddComponent<StorageLifecycle>();
        }
    }
}
