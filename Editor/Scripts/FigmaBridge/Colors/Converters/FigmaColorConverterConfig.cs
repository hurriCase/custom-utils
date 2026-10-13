using CustomUtils.Collections.Scripts;
using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.Theme;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Colors.Converters
{
    [Resource(name: ResourcePaths.FigmaColorConverterConfigAssetName, isEditorResource: true)]
    internal sealed class FigmaColorConverterConfig : SingletonScriptableObject<FigmaColorConverterConfig>
    {
        [field: SerializeField] internal EnumArray<ColorType, FigmaColorConverterBase> Converters { get; private set; }
    }
}
