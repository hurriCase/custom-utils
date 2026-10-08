using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Halftone
{
    /// <summary>
    /// Mirrors Figma's paint blend modes; values must match the HALFTONE_BLEND_* defines in HalftoneUtils.hlsl.
    /// </summary>
    [PublicAPI]
    public enum HalftoneBlendMode
    {
        Normal = 0,
        Darken = 1,
        Multiply = 2,
        ColorBurn = 3,
        LinearBurn = 4,
        Lighten = 5,
        Screen = 6,
        ColorDodge = 7,
        LinearDodge = 8,
        Overlay = 9,
        SoftLight = 10,
        HardLight = 11,
        Difference = 12,
        Exclusion = 13,
        Hue = 14,
        Saturation = 15,
        Color = 16,
        Luminosity = 17
    }
}
