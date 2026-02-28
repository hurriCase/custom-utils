#if CUSTOM_LOCALIZATION
using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using UnityEngine;
using ZLinq;

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
        internal SerializedDictionary<string, LocalizationEntry> Entries { get; private set; } = new();
        [field: SerializeField] public List<SystemLanguage> SupportedLanguages { get; private set; } = new();

        internal IReadOnlyDictionary<string, List<string>> TableToGuids => _tableToGuids;

        [field: SerializeField, HideInInspector]
        private SerializedDictionary<string, List<string>> _tableToGuids = new();

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