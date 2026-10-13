using System;
using CustomUtils.Runtime.Attributes;
using UnityEngine;

#if MULTI_THEME
using CustomUtils.Collections.Scripts;
#endif

namespace CustomUtils.Runtime.UI.Theme.ThemeColors
{
    [Serializable]
    internal sealed class ThemeGradientColor : IThemeColor<Gradient>
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField, InspectorReadOnly] public string Guid { get; private set; }
#if MULTI_THEME
        [field: SerializeField] public EnumArray<ThemeType, Gradient> Colors { get; private set; }
#else
        [field: SerializeField] public Gradient Color { get; private set; }
#endif

#if UNITY_EDITOR
#if MULTI_THEME
        internal static ThemeGradientColor Create(string name, Gradient color)
            => new() { Name = name, Colors = new(color) };
#else
        internal static ThemeGradientColor Create(string name, Gradient color) => new() { Name = name, Color = color };
#endif
#endif

#if UNITY_EDITOR
        public bool TrySetGuid(bool forceRegenerate)
        {
            if (!forceRegenerate && !string.IsNullOrEmpty(Guid))
                return false;

            Guid = System.Guid.NewGuid().ToString();
            return true;
        }
#endif
    }
}