using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage
{
    internal struct ProceduralImageInfo
    {
        internal float Width { get; }
        internal float Height { get; }
        internal float FallOffDistance { get; }
        internal Vector4 NormalizedRadius { get; }
        internal float NormalizedSkew { get; }
        internal float NormalizedBorderWidth { get; }
        internal float NormalizedPixelSize { get; }

        internal ProceduralImageInfo(
            float width,
            float height,
            float fallOffDistance,
            float normalizedPixelSize,
            Vector4 normalizedRadius,
            float normalizedBorderWidth,
            float normalizedSkew)
        {
            Width = Mathf.Abs(width);
            Height = Mathf.Abs(height);
            FallOffDistance = Mathf.Max(0, fallOffDistance);
            NormalizedRadius = normalizedRadius;
            NormalizedBorderWidth = Mathf.Clamp01(normalizedBorderWidth);
            NormalizedPixelSize = Mathf.Clamp01(normalizedPixelSize);
            NormalizedSkew = normalizedSkew;
        }
    }
}