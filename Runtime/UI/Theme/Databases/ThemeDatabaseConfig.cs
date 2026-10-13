using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.Databases
{
    [Resource(
        ResourcePaths.ThemeFullPath,
        ResourcePaths.ThemeDatabaseConfigAssetName,
        ResourcePaths.ThemeResourcePath
    )]
    internal sealed class ThemeDatabaseConfig : SingletonScriptableObject<ThemeDatabaseConfig>
    {
        [field: SerializeField] internal SolidColorDatabase Solid { get; private set; }
        [field: SerializeField] internal GradientColorDatabase Gradient { get; private set; }

        internal IThemeDatabase GetDatabase(ColorType colorType) => colorType switch
        {
            ColorType.Solid => Solid,
            ColorType.Gradient => Gradient,
            _ => null
        };
    }
}
