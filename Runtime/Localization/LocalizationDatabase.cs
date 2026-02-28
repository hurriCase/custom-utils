#if CUSTOM_LOCALIZATION
using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.Downloader;
using CustomUtils.Runtime.Other;
using UnityEngine;

namespace CustomUtils.Runtime.Localization
{
    [Resource(
        ResourcePaths.LocalizationSettingsFullPath,
        ResourcePaths.LocalizationSettingsAssetName,
        ResourcePaths.LocalizationSettingsResourcesPath
    )]
    internal sealed class LocalizationDatabase : SheetsDatabase<LocalizationDatabase, Sheet>
    {
        [field: SerializeField] internal SystemLanguage DefaultLanguage { get; set; }
            = SystemLanguage.English;

        public override string GetDownloadPath() => ResourcePaths.LocalizationSheetsPath;
    }
}
#endif