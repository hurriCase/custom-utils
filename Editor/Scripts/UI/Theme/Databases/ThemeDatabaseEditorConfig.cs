using CustomUtils.Collections.Scripts;
using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.Theme;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.Theme.Databases
{
    [Resource(name: ResourcePaths.ThemeDatabaseEditorConfigAssetName, isEditorResource: true)]
    internal sealed class ThemeDatabaseEditorConfig : SingletonScriptableObject<ThemeDatabaseEditorConfig>
    {
        [field: SerializeField] internal EnumArray<ColorType, ThemeDatabaseEditorBase> Editors { get; private set; }
    }
}
