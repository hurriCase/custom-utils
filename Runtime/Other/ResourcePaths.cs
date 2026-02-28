namespace CustomUtils.Runtime.Other
{
    internal static class ResourcePaths
    {
        internal const string CustomMenuResourcePath = "CustomMenu";
        internal const string CustomMenuSettingsAssetName = "CustomMenuSettings";

        internal const string ThemeFullPath = ResourcesPath + "/" + ThemeResourcePath;
        internal const string SolidColorDatabaseAssetName = "SolidColorDatabase";
        internal const string GradientColorDatabaseAssetName = "GradientColorDatabase";
        internal const string ThemeResourcePath = "Theme";

        internal const string ImagePixelPerUnitFullPath = ResourcesPath + "/" + ImagePixelPerUnitResourcePath;
        internal const string ImagePixelPerUnitDatabaseAssetName = "ImagePixelPerUnitDatabase";
        internal const string ImagePixelPerUnitResourcePath = "ImagePixelPerUnit";

        internal const string LocalizationSettingsFullPath = ResourcesPath + "/" + LocalizationSettingsResourcesPath;
        internal const string LocalizationSettingsAssetName = "LocalizationDatabase";
        internal const string LocalizationRegistryAssetName = "LocalizationRegistry";
        internal const string LocalizationSettingsResourcesPath = "CustomLocalization";
        internal const string LocalizationSheetsPath =
            ResourcesPath + "/" + LocalizationSettingsResourcesPath + "/" + "Localization";

        internal const string AssetLoaderConfigFullPath = ResourcesPath + "/" + AssetLoaderConfigResourcesPath;
        internal const string AssetLoaderConfigAssetName = "AssetLoaderConfig";
        internal const string AssetLoaderConfigResourcesPath = "AssetLoader";

        internal const string LoggerSettingsResourcesPath = "Logger";
        internal const string LoggerSettingsFullPath = ResourcesPath + LoggerSettingsResourcesPath;
        internal const string LoggerSettingsAssetName = "LogCollectorSettings";

        internal const string MappingsPath = "Mappings/";

        private const string ResourcesPath = "Assets/Resources";
    }
}