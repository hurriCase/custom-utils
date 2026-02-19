using CustomUtils.Runtime.CustomTypes.Collections;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Unsafe;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Localization
{
    [PublicAPI]
    public sealed class LanguageLocalizationConfig : SingletonScriptableObject<LanguageLocalizationConfig>
    {
        [SerializeField] private EnumArray<SystemLanguage, string> _localizations;

        public string GetLocalization(SystemLanguage currentEnum)
        {
            var enumValue = UnsafeEnumConverter<SystemLanguage>.ToInt32(currentEnum);
            return _localizations[enumValue];
        }
    }
}