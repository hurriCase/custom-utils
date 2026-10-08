#if UNITY_EDITOR
using System.Collections.Generic;
using CustomUtils.Runtime.Storage.Base;
using UnityEngine;

namespace CustomUtils.Runtime.Storage.Persistent
{
    internal static class StorageKeyRegistry
    {
        private static readonly HashSet<(IStorageProvider provider, string key)> _activeKeys = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _activeKeys.Clear();
        }

        internal static bool TryRegister(IStorageProvider provider, string key)
        {
            // Edit-mode values (e.g. editor windows) are recreated often without being disposed
            if (!Application.isPlaying)
                return false;

            if (_activeKeys.Add((provider, key)))
                return true;

            Debug.LogError($"[{nameof(StorageKeyRegistry)}::{nameof(TryRegister)}] Key '{key}' is already used " +
                           "by another persistent value on the same provider, they will overwrite each other's data");
            return false;
        }

        internal static void Unregister(IStorageProvider provider, string key)
        {
            _activeKeys.Remove((provider, key));
        }
    }
}
#endif
