using System;
using CustomUtils.Runtime.Attributes.ShowIf;
using CustomUtils.Runtime.UI.Theme.Databases;
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

        [SerializeField] private bool _constructFromSolids;
        [SerializeField, ShowIf(nameof(_constructFromSolids))]
        private ColorData _startSolidName;

        [SerializeField, ShowIf(nameof(_constructFromSolids))]
        private ColorData _endSolidName;

#if UNITY_EDITOR && !MULTI_THEME
        internal void TryBakeFromSolids()
        {
            if (!_constructFromSolids)
                return;

            if (!SolidColorDatabase.Instance.TryGetColorByName(_startSolidName.ColorName, out var startColor))
                return;

            if (!SolidColorDatabase.Instance.TryGetColorByName(_endSolidName.ColorName, out var endColor))
                return;

            var existingKeys = Color?.colorKeys;

            if (existingKeys != null && existingKeys[0].color == startColor && existingKeys[1].color == endColor)
                return;

            var gradient = new Gradient();
            var color = new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) };
            var alpha = new[] { new GradientAlphaKey(startColor.a, 0f), new GradientAlphaKey(endColor.a, 1f) };
            gradient.SetKeys(color, alpha);

            Color = gradient;
        }
#endif
    }
}