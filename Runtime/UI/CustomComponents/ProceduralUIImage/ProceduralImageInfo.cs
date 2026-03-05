using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage
{
    internal struct ProceduralImageInfo
    {
        internal float Width { get; }
        internal float Height { get; }
        internal float FallOffDistance { get; }
        internal float NormalizedBorderWidth { get; }
        internal float PixelSize { get; }

        internal ProceduralImageInfo(
            float width,
            float height,
            float fallOffDistance,
            float pixelSize,
            float normalizedBorderWidth)
        {
            Width = Mathf.Abs(width);
            Height = Mathf.Abs(height);
            FallOffDistance = Mathf.Max(0, fallOffDistance);
            NormalizedBorderWidth = Mathf.Clamp01(normalizedBorderWidth);
            PixelSize = Mathf.Max(0, pixelSize);
        }
    }
}