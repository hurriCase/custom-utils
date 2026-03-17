using System;
using CustomUtils.Runtime.Attributes;
using UnityEngine;

#if MULTI_THEME
using CustomUtils.Runtime.CustomTypes.Collections;
#endif

namespace CustomUtils.Runtime.UI.Theme.ThemeColors
{
    [Serializable]
    internal sealed class ThemeSolidColor : IThemeColor<Color>
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField, InspectorReadOnly] public string Guid { get; private set; }
#if MULTI_THEME
        [field: SerializeField] public EnumArray<ThemeType, Color> Colors { get; private set; }
#else
        [field: SerializeField] public Color Color { get; private set; }
#endif

#if UNITY_EDITOR
        public void EnsureGuid()
        {
            if (string.IsNullOrEmpty(Guid))
                Guid = System.Guid.NewGuid().ToString();
        }
#endif
    }
}