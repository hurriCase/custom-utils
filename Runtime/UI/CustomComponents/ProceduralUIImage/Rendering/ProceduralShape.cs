using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Corners;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Rendering
{
    // Edge distances are negative inside the shape.
    internal readonly struct ProceduralShape
    {
        internal Vector2 TopLeft { get; }
        internal Vector2 TopRight { get; }
        internal Vector2 BottomRight { get; }
        internal Vector2 BottomLeft { get; }
        internal Vector4 Radii { get; }

        private const float MinEdgeLength = 0.001f;

        private readonly Vector2 _topNormal;
        private readonly Vector2 _rightNormal;
        private readonly Vector2 _bottomNormal;
        private readonly Vector2 _leftNormal;

        internal ProceduralShape(Rect rect, CornerOffsets offsets, Vector4 radii)
        {
            var pixelOffsets = offsets.ConvertTo(CornerOffsetMode.Absolute, rect.size);

            this = new ProceduralShape(
                new Vector2(rect.xMin, rect.yMax) + pixelOffsets.TopLeft,
                new Vector2(rect.xMax, rect.yMax) + pixelOffsets.TopRight,
                new Vector2(rect.xMax, rect.yMin) + pixelOffsets.BottomRight,
                new Vector2(rect.xMin, rect.yMin) + pixelOffsets.BottomLeft,
                radii);
        }

        private ProceduralShape(
            Vector2 topLeft,
            Vector2 topRight,
            Vector2 bottomRight,
            Vector2 bottomLeft,
            Vector4 radii) : this()
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            Radii = radii;

            var orientation = CalculateOrientation();
            _topNormal = CalculateEdgeNormal(topLeft, topRight, orientation);
            _rightNormal = CalculateEdgeNormal(topRight, bottomRight, orientation);
            _bottomNormal = CalculateEdgeNormal(bottomRight, bottomLeft, orientation);
            _leftNormal = CalculateEdgeNormal(bottomLeft, topLeft, orientation);
        }

        internal ProceduralShape Translate(Vector2 offset)
            => new(TopLeft + offset, TopRight + offset, BottomRight + offset, BottomLeft + offset, Radii);

        internal Vector4 EdgeDistances(Vector2 position)
            => new Vector4(
                Vector2.Dot(position - TopLeft, _topNormal),
                Vector2.Dot(position - TopRight, _rightNormal),
                Vector2.Dot(position - BottomRight, _bottomNormal),
                Vector2.Dot(position - BottomLeft, _leftNormal));

        // 1 when the corners run counterclockwise, -1 when clockwise, so edge normals always point outwards.
        private float CalculateOrientation()
        {
            var doubleArea = Cross(TopLeft, TopRight) + Cross(TopRight, BottomRight) +
                             Cross(BottomRight, BottomLeft) + Cross(BottomLeft, TopLeft);
            return doubleArea < 0f ? 1f : -1f;
        }

        private Vector2 CalculateEdgeNormal(Vector2 start, Vector2 end, float orientation)
        {
            var edge = end - start;
            var direction = edge / Mathf.Max(edge.magnitude, MinEdgeLength);
            return new Vector2(-direction.y, direction.x) * orientation;
        }

        private float Cross(Vector2 left, Vector2 right) => left.x * right.y - left.y * right.x;
    }
}
