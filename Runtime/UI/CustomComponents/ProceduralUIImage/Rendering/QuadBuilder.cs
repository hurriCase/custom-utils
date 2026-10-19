using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Rendering
{
    internal static class QuadBuilder
    {
        private const int SimpleQuadVertexCount = 4;

        internal static bool IsSimpleQuad(VertexHelper vertexHelper)
            => vertexHelper.currentVertCount == SimpleQuadVertexCount;

        internal static void FitToShape(VertexHelper vertexHelper, Rect rect, ProceduralShape shape, float margin)
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
    }
}
