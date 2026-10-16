using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Corners
{
    /// <summary>
    /// Offsets that move the shape's corners, stored in pixels or as a fraction of the rect size.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public struct CornerOffsets
    {
        [field: SerializeField] public Vector2 TopLeft { get; private set; }
        [field: SerializeField] public Vector2 TopRight { get; private set; }
        [field: SerializeField] public Vector2 BottomRight { get; private set; }
        [field: SerializeField] public Vector2 BottomLeft { get; private set; }
        [field: SerializeField] public CornerOffsetMode Mode { get; private set; }

        public Vector2 this[Corner corner] => corner switch
        {
            Corner.TopLeft => TopLeft,
            Corner.TopRight => TopRight,
            Corner.BottomRight => BottomRight,
            _ => BottomLeft
        };

        public CornerOffsets(
            Vector2 topLeft,
            Vector2 topRight,
            Vector2 bottomRight,
            Vector2 bottomLeft,
            CornerOffsetMode mode)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            Mode = mode;
        }

        /// <summary>
        /// Returns the offsets stored in another mode, keeping the corners where they are for this rect size.
        /// </summary>
        public CornerOffsets ConvertTo(CornerOffsetMode mode, Vector2 size)
        {
            if (mode == Mode)
                return this;

            var inverseSize = new Vector2(
                Mathf.Approximately(size.x, 0f) ? 0f : 1f / size.x,
                Mathf.Approximately(size.y, 0f) ? 0f : 1f / size.y);
            var scale = mode == CornerOffsetMode.Relative ? inverseSize : size;

            return new CornerOffsets(
                Vector2.Scale(TopLeft, scale),
                Vector2.Scale(TopRight, scale),
                Vector2.Scale(BottomRight, scale),
                Vector2.Scale(BottomLeft, scale),
                mode);
        }

        public CornerOffsets With(Corner corner, Vector2 offset) => corner switch
        {
            Corner.TopLeft => new CornerOffsets(offset, TopRight, BottomRight, BottomLeft, Mode),
            Corner.TopRight => new CornerOffsets(TopLeft, offset, BottomRight, BottomLeft, Mode),
            Corner.BottomRight => new CornerOffsets(TopLeft, TopRight, offset, BottomLeft, Mode),
            _ => new CornerOffsets(TopLeft, TopRight, BottomRight, offset, Mode)
        };
    }
}
