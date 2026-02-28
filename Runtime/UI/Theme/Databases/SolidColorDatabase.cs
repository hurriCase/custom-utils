using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using CustomUtils.Runtime.UI.Theme.ThemeColors;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.Databases
{
    [Resource(
        ResourcePaths.ThemeFullPath,
        ResourcePaths.SolidColorDatabaseAssetName,
        ResourcePaths.ThemeResourcePath
    )]
    internal sealed class SolidColorDatabase : ThemeColorDatabaseBase<SolidColorDatabase, ThemeSolidColor, Color> { }
}