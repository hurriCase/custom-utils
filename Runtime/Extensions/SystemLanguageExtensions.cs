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
            [SystemLanguage.Afrikaans] = ("af", "afr"),
            [SystemLanguage.Arabic] = ("ar", "ara"),
            [SystemLanguage.Basque] = ("eu", "eus"),
            [SystemLanguage.Belarusian] = ("be", "bel"),
            [SystemLanguage.Bulgarian] = ("bg", "bul"),
            [SystemLanguage.Catalan] = ("ca", "cat"),
            [SystemLanguage.Chinese] = ("zh", "zho"),
            [SystemLanguage.Czech] = ("cs", "ces"),
            [SystemLanguage.Danish] = ("da", "dan"),
            [SystemLanguage.Dutch] = ("nl", "nld"),
            [SystemLanguage.English] = ("en", "eng"),
            [SystemLanguage.Estonian] = ("et", "est"),
            [SystemLanguage.Faroese] = ("fo", "fao"),
            [SystemLanguage.Finnish] = ("fi", "fin"),
            [SystemLanguage.French] = ("fr", "fra"),
            [SystemLanguage.German] = ("de", "deu"),
            [SystemLanguage.Greek] = ("el", "ell"),
            [SystemLanguage.Hebrew] = ("he", "heb"),
            [SystemLanguage.Hungarian] = ("hu", "hun"),
            [SystemLanguage.Icelandic] = ("is", "isl"),
            [SystemLanguage.Indonesian] = ("id", "ind"),
            [SystemLanguage.Italian] = ("it", "ita"),
            [SystemLanguage.Japanese] = ("ja", "jpn"),
            [SystemLanguage.Korean] = ("ko", "kor"),
            [SystemLanguage.Latvian] = ("lv", "lav"),
            [SystemLanguage.Lithuanian] = ("lt", "lit"),
            [SystemLanguage.Norwegian] = ("nb", "nor"),
            [SystemLanguage.Polish] = ("pl", "pol"),
            [SystemLanguage.Portuguese] = ("pt", "por"),
            [SystemLanguage.Romanian] = ("ro", "ron"),
            [SystemLanguage.Russian] = ("ru", "rus"),
            [SystemLanguage.SerboCroatian] = ("sr", "hbs"),
            [SystemLanguage.Slovak] = ("sk", "slk"),
            [SystemLanguage.Slovenian] = ("sl", "slv"),
            [SystemLanguage.Spanish] = ("es", "spa"),
            [SystemLanguage.Swedish] = ("sv", "swe"),
            [SystemLanguage.Thai] = ("th", "tha"),
            [SystemLanguage.Turkish] = ("tr", "tur"),
            [SystemLanguage.Ukrainian] = ("uk", "ukr"),
            [SystemLanguage.Vietnamese] = ("vi", "vie"),
            [SystemLanguage.ChineseSimplified] = ("zh", "zhs"),
            [SystemLanguage.ChineseTraditional] = ("zh", "zht"),
            [SystemLanguage.Hindi] = ("hi", "hin"),
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
        public static string ToLocaleCode(this SystemLanguage language) =>
            language switch
            {
                SystemLanguage.Afrikaans => "af-ZA",
                SystemLanguage.Arabic => "ar-SA",
                SystemLanguage.Basque => "eu-ES",
                SystemLanguage.Belarusian => "be-BY",
                SystemLanguage.Bulgarian => "bg-BG",
                SystemLanguage.Catalan => "ca-ES",
                SystemLanguage.Chinese => "zh-CN",
                SystemLanguage.Czech => "cs-CZ",
                SystemLanguage.Danish => "da-DK",
                SystemLanguage.Dutch => "nl-NL",
                SystemLanguage.English => "en-US",
                SystemLanguage.Estonian => "et-EE",
                SystemLanguage.Faroese => "fo-FO",
                SystemLanguage.Finnish => "fi-FI",
                SystemLanguage.French => "fr-FR",
                SystemLanguage.German => "de-DE",
                SystemLanguage.Greek => "el-GR",
                SystemLanguage.Hebrew => "he-IL",
                SystemLanguage.Hungarian => "hu-HU",
                SystemLanguage.Icelandic => "is-IS",
                SystemLanguage.Indonesian => "id-ID",
                SystemLanguage.Italian => "it-IT",
                SystemLanguage.Japanese => "ja-JP",
                SystemLanguage.Korean => "ko-KR",
                SystemLanguage.Latvian => "lv-LV",
                SystemLanguage.Lithuanian => "lt-LT",
                SystemLanguage.Norwegian => "nb-NO",
                SystemLanguage.Polish => "pl-PL",
                SystemLanguage.Portuguese => "pt-BR",
                SystemLanguage.Romanian => "ro-RO",
                SystemLanguage.Russian => "ru-RU",
                SystemLanguage.SerboCroatian => "sr-RS",
                SystemLanguage.Slovak => "sk-SK",
                SystemLanguage.Slovenian => "sl-SI",
                SystemLanguage.Spanish => "es-ES",
                SystemLanguage.Swedish => "sv-SE",
                SystemLanguage.Thai => "th-TH",
                SystemLanguage.Turkish => "tr-TR",
                SystemLanguage.Ukrainian => "uk-UA",
                SystemLanguage.Vietnamese => "vi-VN",
                SystemLanguage.ChineseSimplified => "zh-CN",
                SystemLanguage.ChineseTraditional => "zh-TW",
                SystemLanguage.Hindi => "hi-IN",
                _ => "en-US"
            };

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