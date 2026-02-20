using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="SystemLanguage"/>.
    /// </summary>
    [PublicAPI]
    public static class SystemLanguageExtensions
    {
        private static readonly Dictionary<SystemLanguage, (string iso1, string iso2)> _languageCodes = new()
        {
            [SystemLanguage.English] = ("en", "eng"),
            [SystemLanguage.Japanese] = ("ja", "jpn"),
            [SystemLanguage.Korean] = ("ko", "kor"),
            [SystemLanguage.Chinese] = ("zh", "zho"),
            [SystemLanguage.ChineseSimplified] = ("zh", "zhs"),
            [SystemLanguage.ChineseTraditional] = ("zh", "zht"),
            [SystemLanguage.Spanish] = ("es", "spa"),
            [SystemLanguage.French] = ("fr", "fra"),
            [SystemLanguage.German] = ("de", "deu"),
            [SystemLanguage.Italian] = ("it", "ita"),
            [SystemLanguage.Portuguese] = ("pt", "por"),
            [SystemLanguage.Russian] = ("ru", "rus"),
            [SystemLanguage.Arabic] = ("ar", "ara"),
            [SystemLanguage.Hindi] = ("hi", "hin"),
            [SystemLanguage.Thai] = ("th", "tha"),
            [SystemLanguage.Vietnamese] = ("vi", "vie"),
            [SystemLanguage.Turkish] = ("tr", "tur"),
            [SystemLanguage.Polish] = ("pl", "pol"),
            [SystemLanguage.Dutch] = ("nl", "nld"),
            [SystemLanguage.Swedish] = ("sv", "swe"),
            [SystemLanguage.Norwegian] = ("nb", "nor"),
            [SystemLanguage.Danish] = ("da", "dan"),
            [SystemLanguage.Finnish] = ("fi", "fin"),
            [SystemLanguage.Czech] = ("cs", "ces"),
            [SystemLanguage.Hungarian] = ("hu", "hun"),
            [SystemLanguage.Greek] = ("el", "ell"),
            [SystemLanguage.Hebrew] = ("he", "heb"),
            [SystemLanguage.Bulgarian] = ("bg", "bul"),
            [SystemLanguage.Romanian] = ("ro", "ron"),
            [SystemLanguage.Slovak] = ("sk", "slk"),
            [SystemLanguage.Slovenian] = ("sl", "slv"),
            [SystemLanguage.Ukrainian] = ("uk", "ukr"),
            [SystemLanguage.Lithuanian] = ("lt", "lit"),
            [SystemLanguage.Latvian] = ("lv", "lav"),
            [SystemLanguage.Estonian] = ("et", "est")
        };

        private static Dictionary<string, SystemLanguage> _reverseMap;

        /// <summary>
        /// Converts a Unity SystemLanguage enum value to its corresponding ISO 639-1 language code (2-letter format).
        /// </summary>
        /// <param name="language">The SystemLanguage to convert.</param>
        /// <returns>The ISO 639-1 language code (e.g., "en", "ja"), or "en" if not found.</returns>
        public static string SystemLanguageToISO1(this SystemLanguage language)
            => _languageCodes.TryGetValue(language, out var codes) ? codes.iso1 : "en";

        /// <summary>
        /// Converts a Unity SystemLanguage enum value to its corresponding ISO 639-2 language code (3-letter format).
        /// </summary>
        /// <param name="language">The SystemLanguage to convert.</param>
        /// <returns>The ISO 639-2 language code (e.g., "eng", "spa"), or "eng" if not found.</returns>
        public static string SystemLanguageToISO2(this SystemLanguage language)
            => _languageCodes.TryGetValue(language, out var codes) ? codes.iso2 : "eng";

        /// <summary>
        /// Converts an ISO language code (either 2-letter or 3-letter) to its corresponding Unity SystemLanguage.
        /// </summary>
        /// <param name="isoCode">The ISO language code to convert.</param>
        /// <returns>The corresponding SystemLanguage, or SystemLanguage.English if not found.</returns>
        public static SystemLanguage ISOToSystemLanguage(this string isoCode)
            => GetReverseMap().GetValueOrDefault(isoCode, SystemLanguage.English);

        /// <summary>
        /// Converts a Unity SystemLanguage to a locale code in the format "language-REGION" (e.g., "en-US", "ja-JP").
        /// This format is compatible with speech recognition APIs, localization systems, and web standards.
        /// </summary>
        /// <param name="language">The SystemLanguage to convert.</param>
        /// <returns>The locale code string, or "en-US" if not found.</returns>
        public static string ToLocaleCode(this SystemLanguage language)
        {
            return language switch
            {
                SystemLanguage.English => "en-US",
                SystemLanguage.Japanese => "ja-JP",
                SystemLanguage.Russian => "ru-RU",
                SystemLanguage.Spanish => "es-ES",
                SystemLanguage.French => "fr-FR",
                SystemLanguage.German => "de-DE",
                SystemLanguage.Korean => "ko-KR",
                SystemLanguage.Chinese => "zh-CN",
                SystemLanguage.ChineseSimplified => "zh-CN",
                SystemLanguage.ChineseTraditional => "zh-TW",
                SystemLanguage.Portuguese => "pt-BR",
                SystemLanguage.Italian => "it-IT",
                SystemLanguage.Polish => "pl-PL",
                SystemLanguage.Turkish => "tr-TR",
                SystemLanguage.Dutch => "nl-NL",
                SystemLanguage.Swedish => "sv-SE",
                SystemLanguage.Norwegian => "nb-NO",
                SystemLanguage.Danish => "da-DK",
                SystemLanguage.Finnish => "fi-FI",
                SystemLanguage.Czech => "cs-CZ",
                SystemLanguage.Hungarian => "hu-HU",
                SystemLanguage.Greek => "el-GR",
                SystemLanguage.Hebrew => "he-IL",
                SystemLanguage.Thai => "th-TH",
                SystemLanguage.Vietnamese => "vi-VN",
                SystemLanguage.Ukrainian => "uk-UA",
                SystemLanguage.Arabic => "ar-SA",
                SystemLanguage.Hindi => "hi-IN",
                SystemLanguage.Bulgarian => "bg-BG",
                SystemLanguage.Romanian => "ro-RO",
                SystemLanguage.Slovak => "sk-SK",
                SystemLanguage.Slovenian => "sl-SI",
                SystemLanguage.Lithuanian => "lt-LT",
                SystemLanguage.Latvian => "lv-LV",
                SystemLanguage.Estonian => "et-EE",
                _ => "en-US"
            };
        }

        private static Dictionary<string, SystemLanguage> GetReverseMap()
        {
            if (_reverseMap != null)
                return _reverseMap;

            _reverseMap = new Dictionary<string, SystemLanguage>();
            foreach (var (language, (iso1, iso2)) in _languageCodes)
            {
                _reverseMap[iso1] = language;
                _reverseMap[iso2] = language;
            }

            return _reverseMap;
        }
    }
}