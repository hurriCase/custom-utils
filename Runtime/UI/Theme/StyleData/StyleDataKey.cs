using System;
using CustomUtils.Runtime.UI.Halftone;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.StyleData
{
    // Value identity of one style data row. Colors stay as guids so rows are re-resolved when the theme changes.
    internal readonly struct StyleDataKey : IEquatable<StyleDataKey>
    {
        internal ColorType ColorType { get; }
        internal string ColorGuid { get; }
        internal Color RawColor { get; }
        internal bool IsColorWritten { get; }
        internal GradientType GradientType { get; }
        internal float GradientRotation { get; }
        internal DiamondGradientDirection DiamondDirection { get; }
        internal bool UseHalftone { get; }
        internal Vector2 PatternOffset { get; }
        internal Vector2 PatternScale { get; }
        internal float PatternOpacity { get; }
        internal HalftoneBlendMode BlendMode { get; }
        internal float PatternRotation { get; }

        private StyleDataKey(
            ColorType colorType,
            string colorGuid,
            Color rawColor,
            bool isColorWritten,
            GradientType gradientType,
            float gradientRotation,
            DiamondGradientDirection diamondDirection,
            HalftoneProperties halftoneProperties)
        {
            ColorType = colorType;
            ColorGuid = colorGuid;
            RawColor = rawColor;
            IsColorWritten = isColorWritten;
            GradientType = gradientType;
            GradientRotation = gradientRotation;
            DiamondDirection = diamondDirection;
            UseHalftone = halftoneProperties != null;
            PatternOffset = halftoneProperties?.PatternOffset ?? Vector2.zero;
            PatternScale = halftoneProperties?.PatternScale ?? Vector2.zero;
            PatternOpacity = halftoneProperties?.PatternOpacity ?? 0f;
            BlendMode = halftoneProperties?.BlendMode ?? 0;
            PatternRotation = halftoneProperties?.PatternRotation ?? 0f;
        }

        // A fill gets its solid color from the vertex color (set by the solid color modifier), so it isn't written.
        internal static StyleDataKey FromStyle(ThemeStyle style, bool isColorWritten)
        {
            var halftoneProperties = style.UseHalftone ? style.HalftoneProperties : null;
            var colorData = style.ColorData;

            return new StyleDataKey(
                colorData.ColorType,
                colorData.Guid,
                Color.white,
                isColorWritten,
                style.GradientType,
                style.GradientRotation,
                style.DiamondDirection,
                halftoneProperties);
        }

        internal static StyleDataKey FromColor(Color color)
            => new(ColorType.Solid, null, color, true, default, default, default, null);

        internal bool HasGradient => ColorType == ColorType.Gradient && !string.IsNullOrEmpty(ColorGuid);

        internal bool HasNoEffect => !HasGradient && !UseHalftone && !IsColorWritten;

        public bool Equals(StyleDataKey other)
            => ColorType == other.ColorType
               && ColorGuid == other.ColorGuid
               && RawColor == other.RawColor
               && IsColorWritten == other.IsColorWritten
               && GradientType == other.GradientType
               && GradientRotation.Equals(other.GradientRotation)
               && DiamondDirection == other.DiamondDirection
               && UseHalftone == other.UseHalftone
               && PatternOffset == other.PatternOffset
               && PatternScale == other.PatternScale
               && PatternOpacity.Equals(other.PatternOpacity)
               && BlendMode == other.BlendMode
               && PatternRotation.Equals(other.PatternRotation);

        public override bool Equals(object obj) => obj is StyleDataKey other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add((int)ColorType);
            hash.Add(ColorGuid);
            hash.Add(RawColor);
            hash.Add(IsColorWritten);
            hash.Add((int)GradientType);
            hash.Add(GradientRotation);
            hash.Add((int)DiamondDirection);
            hash.Add(UseHalftone);
            hash.Add(PatternOffset);
            hash.Add(PatternScale);
            hash.Add(PatternOpacity);
            hash.Add((int)BlendMode);
            hash.Add(PatternRotation);
            return hash.ToHashCode();
        }
    }
}
