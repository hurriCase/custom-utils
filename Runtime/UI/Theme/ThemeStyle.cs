using System;
using CustomUtils.Runtime.Attributes.ShowIf;
using CustomUtils.Runtime.UI.Halftone;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme
{
    /// <summary>
    /// Full visual style of a themed graphic: its theme color, gradient direction and optional halftone pattern.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public struct ThemeStyle
    {
        /// <summary>
        /// Gets the theme color type and guid.
        /// </summary>
        [field: SerializeField] public ColorData ColorData { get; private set; }

        [field: SerializeField] internal GradientType GradientType { get; private set; }

        [field: SerializeField, ShowIf(nameof(GradientType), GradientType.Linear, GradientType.Angular)]
        internal float GradientRotation { get; private set; }

        [field: SerializeField, ShowIf(nameof(GradientType), GradientType.Diamond)]
        internal DiamondGradientDirection DiamondDirection { get; private set; }

        /// <summary>
        /// Gets whether the halftone pattern is applied.
        /// </summary>
        [field: SerializeField] public bool UseHalftone { get; private set; }

        /// <summary>
        /// Gets the halftone pattern settings, used when <see cref="UseHalftone"/> is enabled.
        /// </summary>
        [field: SerializeField, ShowIf(nameof(UseHalftone))]
        public HalftoneProperties HalftoneProperties { get; private set; }

        internal ThemeStyle(
            ColorData colorData,
            GradientType gradientType,
            float gradientRotation,
            DiamondGradientDirection diamondDirection,
            bool useHalftone,
            HalftoneProperties halftoneProperties)
        {
            ColorData = colorData;
            GradientType = gradientType;
            GradientRotation = gradientRotation;
            DiamondDirection = diamondDirection;
            UseHalftone = useHalftone;
            HalftoneProperties = halftoneProperties;
        }
    }
}
