#if CUSTOM_LOCALIZATION
using System;
using AYellowpaper.SerializedCollections;
using CustomUtils.Runtime.Attributes;
using UnityEngine;

namespace CustomUtils.Runtime.Localization
{
    [Serializable]
    internal sealed class LocalizationEntry
    {
        [field: SerializeField, InspectorReadOnly] internal string Key { get; private set; }
        [field: SerializeField, InspectorReadOnly] internal string GUID { get; private set; }
        [field: SerializeField, InspectorReadOnly] internal string TableName { get; private set; }

        [field: SerializeField, InspectorReadOnly]
        internal SerializedDictionary<SystemLanguage, string> Translations { get; private set; } = new();

        internal bool IsValid => string.IsNullOrEmpty(Key) is false
                                 && string.IsNullOrEmpty(GUID) is false
                                 && string.IsNullOrEmpty(TableName) is false;

        internal LocalizationEntry(string guid, string key, string tableName)
        {
            GUID = guid;
            Key = key;
            TableName = tableName;
        }

        internal void SetTranslation(SystemLanguage language, string translation)
        {
            Translations[language] = translation;
        }

        internal bool TryGetTranslation(SystemLanguage language, out string translation)
            => Translations.TryGetValue(language, out translation);
    }
}
#endif