using JetBrains.Annotations;
using R3;
using UnityEditor;
using UnityEngine;
using ZLinq;

namespace CustomUtils.Runtime.Localization
{
    /// <summary>
    /// Reactive localization controller with GUID-based key support.
    /// Manages localized text retrieval and language switching for Unity applications.
    /// </summary>
    [PublicAPI]
    public static class LocalizationController
    {
        /// <summary>
        /// Reactive property for current language with automatic localization updates.
        /// </summary>

        public static ReactiveProperty<SystemLanguage> Language { get; } =
            new(LocalizationDatabase.Instance.DefaultLanguage);

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitializeInEditor()
        {
            ReadLocalizationData();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInRuntime()
        {
            var systemLanguage = Application.systemLanguage;
            if (HasLanguage(systemLanguage))
                Language.Value = systemLanguage;
        }

        /// <summary>
        /// Gets the localized text for the specified localization key using the current language.
        /// </summary>
        public static string Localize(LocalizationKey localizationKey) =>
            Localize(localizationKey, Language.Value);

        /// <summary>
        /// Gets the localized text for the specified localization key and language.
        /// </summary>
        public static string Localize(LocalizationKey localizationKey, SystemLanguage language)
        {
            if (localizationKey.IsValid is false
                || LocalizationRegistry.Instance.Entries.TryGetValue(localizationKey.GUID, out var entry) is false)
                return string.Empty;

            if (entry.TryGetTranslation(language, out var translation) &&
                string.IsNullOrEmpty(translation) is false)
                return translation;

            if (entry.TryGetTranslation(SystemLanguage.English, out var fallback) &&
                string.IsNullOrEmpty(fallback) is false)
                return fallback;

            return entry.Key;
        }

        /// <summary>
        /// Checks if localization data exists for the specified language.
        /// </summary>
        public static bool HasLanguage(SystemLanguage language)
            => LocalizationRegistry.Instance.SupportedLanguages.Contains(language);

        /// <summary>
        /// Retrieves all localization keys across all available languages.
        /// </summary>
        public static string[] GetAllKeys() =>
            LocalizationRegistry.Instance.Entries.Values
                .Select(static localizationEntry => localizationEntry.Key)
                .OrderBy(static guid => guid)
                .ToArray();

        internal static void ReadLocalizationData()
        {
            var settings = LocalizationDatabase.Instance;

            if (settings.Sheets is null || settings.Sheets.Count == 0)
            {
                Debug.LogWarning("[LocalizationController] No localization sheets found");
                return;
            }

            LocalizationSheetProcessor.ProcessSheets(settings.Sheets);
        }
    }
}