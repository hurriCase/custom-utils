using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.FilledImage
{
    internal sealed class ArcMeshBuilder
    {
        internal void BuildMesh(VertexHelper vertexHelper, ArcGeometry geometry, Color color)
        {
            vertexHelper.Clear();

            BuildArcSegments(vertexHelper, geometry, color);

            if (!geometry.HasRoundedCaps)
                return;

            BuildSingleCap(vertexHelper, geometry.StartCapPoints, color);
            BuildSingleCap(vertexHelper, geometry.EndCapPoints, color);
        }

        private void BuildArcSegments(VertexHelper vertexHelper, ArcGeometry geometry, Color color)
        {
            for (var i = 0; i < geometry.SegmentCount; i++)
            {
                var vertexStart = vertexHelper.currentVertCount;

                vertexHelper.AddVert(geometry.InnerPoints[i], color, Vector4.zero);
                vertexHelper.AddVert(geometry.OuterPoints[i], color, Vector4.zero);
                vertexHelper.AddVert(geometry.InnerPoints[i + 1], color, Vector4.zero);
                vertexHelper.AddVert(geometry.OuterPoints[i + 1], color, Vector4.zero);

                vertexHelper.AddTriangle(vertexStart, vertexStart + 1, vertexStart + 2);
                vertexHelper.AddTriangle(vertexStart + 1, vertexStart + 3, vertexStart + 2);
            }
        }

        private void BuildSingleCap(VertexHelper vertexHelper, IReadOnlyCollection<Vector2> capPoints, Color color)
        {
            var vertexStart = vertexHelper.currentVertCount;

            foreach (var point in capPoints)
                vertexHelper.AddVert(point, color, Vector2.zero);

            for (var i = 1; i < capPoints.Count - 1; i++)
                vertexHelper.AddTriangle(vertexStart, vertexStart + i, vertexStart + i + 1);
        }
    }
}