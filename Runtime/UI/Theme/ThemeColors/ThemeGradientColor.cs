using System;
using UnityEngine;

#if MULTI_THEME
using CustomUtils.Runtime.CustomTypes.Collections;
#endif

namespace CustomUtils.Runtime.UI.Theme.ThemeColors
{
    [Serializable]
    internal sealed class ThemeGradientColor : IThemeColor<Gradient>
    {
        [field: SerializeField] public string Name { get; private set; }
#if MULTI_THEME
        [field: SerializeField] public EnumArray<ThemeType, Gradient> Colors { get; private set; }
#else
        [field: SerializeField] public Gradient Color { get; private set; }
#endif
    }
}