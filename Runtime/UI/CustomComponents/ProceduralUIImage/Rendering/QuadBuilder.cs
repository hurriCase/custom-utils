using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Shape;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Rendering
{
    internal static class QuadBuilder
    {
        private const int SimpleQuadVertexCount = 4;

        internal static bool IsSimpleQuad(VertexHelper vertexHelper)
            => vertexHelper.currentVertCount == SimpleQuadVertexCount;

        // A clipped image keeps its own rect, cut to the shape; otherwise the quad is bent to the shape's corners.
        internal static void Build(
            VertexHelper vertexHelper,
            Rect rect,
            ProceduralShape shape,
            float margin,
            bool isClipped)
        {
            if (!isClipped)
            {
                FitToShape(vertexHelper, rect, shape, margin);
                return;
            }

            var bounds = shape.Bounds;
            var clipBounds = Rect.MinMaxRect(
                bounds.xMin - margin,
                bounds.yMin - margin,
                bounds.xMax + margin,
                bounds.yMax + margin);
            ClipToBounds(vertexHelper, clipBounds);
        }

        private static void FitToShape(VertexHelper vertexHelper, Rect rect, ProceduralShape shape, float margin)
        {
            var center = rect.center;
            var vertex = new UIVertex();

            for (var i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, i);

                var isRight = vertex.position.x > center.x;
                var isTop = vertex.position.y > center.y;
                var shapeCorner = (isTop, isRight) switch
                {
                    (true, false) => shape.TopLeft,
                    (true, true) => shape.TopRight,
                    (false, true) => shape.BottomRight,
                    _ => shape.BottomLeft
                };
                var rectCorner = new Vector2(isRight ? rect.xMax : rect.xMin, isTop ? rect.yMax : rect.yMin);
                var direction = new Vector2(isRight ? 1f : -1f, isTop ? 1f : -1f);

                vertex.position += (Vector3)(shapeCorner - rectCorner + direction * margin);
                vertexHelper.SetUIVertex(vertex, i);
            }
        }

        // The sprite quad can be smaller than the rect (sprite padding, preserve aspect), so uvs follow the quad.
        private static void ClipToBounds(VertexHelper vertexHelper, Rect bounds)
        {
            if (!IsSimpleQuad(vertexHelper))
                return;

            var firstVertex = new UIVertex();
            var oppositeVertex = new UIVertex();
            vertexHelper.PopulateUIVertex(ref firstVertex, 0);
            vertexHelper.PopulateUIVertex(ref oppositeVertex, 2);
            vertexHelper.Clear();

            var quad = Rect.MinMaxRect(
                firstVertex.position.x,
                firstVertex.position.y,
                oppositeVertex.position.x,
                oppositeVertex.position.y);
            var min = Vector2.Max(quad.min, bounds.min);
            var max = Vector2.Min(quad.max, bounds.max);

            if (min.x >= max.x || min.y >= max.y)
                return;

            var uvMin = (Vector2)firstVertex.uv0;
            var uvMax = (Vector2)oppositeVertex.uv0;

            AddVertex(vertexHelper, quad, min, firstVertex.color, uvMin, uvMax);
            AddVertex(vertexHelper, quad, new Vector2(min.x, max.y), firstVertex.color, uvMin, uvMax);
            AddVertex(vertexHelper, quad, max, firstVertex.color, uvMin, uvMax);
            AddVertex(vertexHelper, quad, new Vector2(max.x, min.y), firstVertex.color, uvMin, uvMax);

            vertexHelper.AddTriangle(0, 1, 2);
            vertexHelper.AddTriangle(2, 3, 0);
        }

        private static void AddVertex(
            VertexHelper vertexHelper,
            Rect quad,
            Vector2 position,
            Color32 color,
            Vector2 uvMin,
            Vector2 uvMax)
        {
            var fraction = Rect.PointToNormalized(quad, position);
            var uv = new Vector2(Mathf.Lerp(uvMin.x, uvMax.x, fraction.x), Mathf.Lerp(uvMin.y, uvMax.y, fraction.y));

            vertexHelper.AddVert(position, color, uv);
        }
    }
}
