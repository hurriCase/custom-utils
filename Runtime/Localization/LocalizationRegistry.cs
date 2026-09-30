#if CUSTOM_LOCALIZATION
using System;
using System.Collections;
using System.Collections.Generic;
using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using UnityEngine;
using ZLinq;

#if !UNITY_6000_6_OR_NEWER
using AYellowpaper.SerializedCollections;
#endif

namespace CustomUtils.Runtime.Localization
{
    [Resource(
        ResourcePaths.LocalizationSettingsFullPath,
        ResourcePaths.LocalizationRegistryAssetName,
        ResourcePaths.LocalizationSettingsResourcesPath
    )]
    public sealed class LocalizationRegistry : SingletonScriptableObject<LocalizationRegistry>
    {
        [field: SerializeField]
#if UNITY_6000_6_OR_NEWER
        internal Dictionary<string, LocalizationEntry> Entries { get; private set; } = new();
#else
        internal SerializedDictionary<string, LocalizationEntry> Entries { get; private set; } = new();
#endif
        [field: SerializeField] public List<SystemLanguage> SupportedLanguages { get; private set; } = new();

        internal IReadOnlyDictionary<string, List<string>> TableToGuids => _tableToGuids;

        [field: SerializeField, HideInInspector]
#if UNITY_6000_6_OR_NEWER
        private Dictionary<string, List<string>> _tableToGuids = new();
#else
        private SerializedDictionary<string, List<string>> _tableToGuids = new();
#endif

        internal void AddOrUpdateEntry(LocalizationEntry entry)
        {
            Entries[entry.GUID] = entry;

            if (!_tableToGuids.ContainsKey(entry.TableName))
                _tableToGuids[entry.TableName] = new List<string>();

            _tableToGuids[entry.TableName].Add(entry.GUID);

            this.MarkAsDirty();
        }

        internal void ReplaceSupportedLanguages(List<SystemLanguage> supportedLanguages)
        {
            SupportedLanguages = supportedLanguages;
            this.MarkAsDirty();
        }

        internal IList SearchEntries(string searchText, string tableName = null)
        {
            var entries = GetEntriesFromTable(tableName);

            if (string.IsNullOrEmpty(searchText))
                return entries;

            return entries
                .Where(entry => entry.Key.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        internal void Clear()
        {
            Entries.Clear();
            _tableToGuids.Clear();
            this.MarkAsDirty();
        }

        private LocalizationEntry[] GetEntriesFromTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return Entries.Values.ToArray();

            return !_tableToGuids.TryGetValue(tableName, out var guids)
                ? Array.Empty<LocalizationEntry>()
                : guids.Select(guid => Entries[guid]).ToArray();
        }
    }
}
#endif