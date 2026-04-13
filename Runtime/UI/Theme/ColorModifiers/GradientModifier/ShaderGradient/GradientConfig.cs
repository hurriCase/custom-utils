using AYellowpaper.SerializedCollections;
using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient
{
    [Resource(name: nameof(GradientConfig))]
    internal sealed class GradientConfig : SingletonScriptableObject<GradientConfig>
    {
        [field: SerializeField] internal SerializedDictionary<GradientType, string> GradientKeywords { get; set; }
    }
}