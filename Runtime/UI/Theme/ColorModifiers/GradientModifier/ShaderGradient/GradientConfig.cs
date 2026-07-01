using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using UnityEngine;

#if UNITY_6000_7_OR_NEWER
using System.Collections.Generic;

#else
using AYellowpaper.SerializedCollections;
#endif

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient
{
    [Resource(name: nameof(GradientConfig))]
    internal sealed class GradientConfig : SingletonScriptableObject<GradientConfig>
    {
#if UNITY_6000_7_OR_NEWER
        [field: SerializeField] internal Dictionary<GradientType, string> GradientKeywords { get; set; }
#else
        [field: SerializeField] internal SerializedDictionary<GradientType, string> GradientKeywords { get; set; }
#endif
    }
}