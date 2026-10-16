using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Corners;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Shape
{
    // Edge distances are negative inside the shape.
    internal readonly struct ProceduralShape
    {
        internal Vector2 TopLeft { get; }
        internal Vector2 TopRight { get; }
        internal Vector2 BottomRight { get; }
        internal Vector2 BottomLeft { get; }
        internal Vector4 Radii { get; }

        internal Vector4 InsetRadii => Vector4.Max(Radii - Vector4.one * _inset, Vector4.zero);

        internal Rect Bounds
        {
            get
            {
                var min = Vector2.Min(Vector2.Min(TopLeft, TopRight), Vector2.Min(BottomRight, BottomLeft));
                var max = Vector2.Max(Vector2.Max(TopLeft, TopRight), Vector2.Max(BottomRight, BottomLeft));
                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }

        private const float MinEdgeLength = 0.001f;

        private readonly float _inset;
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
                radii,
                0f);
        }

        private ProceduralShape(
            Vector2 topLeft,
            Vector2 topRight,
            Vector2 bottomRight,
            Vector2 bottomLeft,
            Vector4 radii,
            float inset) : this()
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            Radii = radii;
            _inset = inset;

            var orientation = CalculateOrientation();
            _topNormal = CalculateEdgeNormal(topLeft, topRight, orientation);
            _rightNormal = CalculateEdgeNormal(topRight, bottomRight, orientation);
            _bottomNormal = CalculateEdgeNormal(bottomRight, bottomLeft, orientation);
            _leftNormal = CalculateEdgeNormal(bottomLeft, topLeft, orientation);
        }

        internal ProceduralShape Translate(Vector2 offset)
            => new(TopLeft + offset, TopRight + offset, BottomRight + offset, BottomLeft + offset, Radii, _inset);

        // Pulls every edge inwards; the radii shrink with it so the corners stay concentric.
        internal ProceduralShape Shrink(float inset)
            => new(TopLeft, TopRight, BottomRight, BottomLeft, Radii, _inset + inset);

        internal ProceduralShape Transform(Transform from, Transform to)
        {
            var unit = from.TransformVector(Vector3.right);
            var scale = to.InverseTransformVector(unit).magnitude;

            return new ProceduralShape(
                MapPoint(from, to, TopLeft),
                MapPoint(from, to, TopRight),
                MapPoint(from, to, BottomRight),
                MapPoint(from, to, BottomLeft),
                Radii * scale,
                _inset * scale);
        }

        internal Vector4 EdgeDistances(Vector2 position)
            => new Vector4(
                Vector2.Dot(position - TopLeft, _topNormal),
                Vector2.Dot(position - TopRight, _rightNormal),
                Vector2.Dot(position - BottomRight, _bottomNormal),
                Vector2.Dot(position - BottomLeft, _leftNormal)) + Vector4.one * _inset;

        // Moving the shape by an offset shifts every edge distance by a constant.
        internal Vector4 EdgeShift(Vector2 offset)
            => new(
                Vector2.Dot(offset, _topNormal),
                Vector2.Dot(offset, _rightNormal),
                Vector2.Dot(offset, _bottomNormal),
                Vector2.Dot(offset, _leftNormal));

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

        private Vector2 MapPoint(Transform from, Transform to, Vector2 point)
        {
            var worldPoint = from.TransformPoint(point);
            return to.InverseTransformPoint(worldPoint);
        }
    }
}
