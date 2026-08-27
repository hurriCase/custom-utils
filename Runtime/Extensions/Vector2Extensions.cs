using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Vector2"/> and polygon vertex collections.
    /// </summary>
    [PublicAPI]
    public static class Vector2Extensions
    {
        private const float MinRayDistance = 1e-4f;
        private const float DenominatorEpsilon = 1e-6f;

        /// <summary>
        /// Computes the 2D cross product (z-component) of two vectors.
        /// </summary>
        /// <param name="vectorA">The first vector.</param>
        /// <param name="vectorB">The second vector.</param>
        /// <returns>A positive value if <paramref name="vectorB"/> is counter-clockwise from <paramref name="vectorA"/>; negative if clockwise; zero if parallel.</returns>
        public static float Cross(this Vector2 vectorA, Vector2 vectorB) =>
            vectorA.x * vectorB.y - vectorA.y * vectorB.x;

        /// <summary>
        /// Rotates a direction vector by the given angle.
        /// </summary>
        /// <param name="direction">The vector to rotate.</param>
        /// <param name="degrees">The rotation angle, in degrees, counter-clockwise.</param>
        /// <returns>The rotated vector.</returns>
        public static Vector2 Rotate(this Vector2 direction, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);

            return new Vector2(direction.x * cos - direction.y * sin, direction.x * sin + direction.y * cos);
        }

        /// <summary>
        /// Gets a point along a polygon edge by linear interpolation.
        /// </summary>
        /// <param name="vertices">The polygon vertices, in order.</param>
        /// <param name="edgeIndex">The index of the edge's starting vertex; the edge runs to the next vertex, wrapping around.</param>
        /// <param name="edgeFraction">The interpolation fraction along the edge, from 0 (start) to 1 (end).</param>
        /// <returns>The interpolated point on the edge.</returns>
        public static Vector2 PointOnEdge(this IReadOnlyList<Vector2> vertices, int edgeIndex, float edgeFraction) =>
            Vector2.Lerp(vertices[edgeIndex], vertices[(edgeIndex + 1) % vertices.Count], edgeFraction);

        /// <summary>
        /// Gets a random point on the polygon's outline, uniformly sampled by edge count (not by edge length).
        /// </summary>
        /// <param name="vertices">The polygon vertices, in order.</param>
        /// <returns>A random point on one of the polygon's edges.</returns>
        public static Vector2 RandomPointOnOutline(this IReadOnlyList<Vector2> vertices) =>
            vertices.PointOnEdge(Random.Range(0, vertices.Count), Random.value);

        /// <summary>
        /// Casts a ray from <paramref name="origin"/> in <paramref name="direction"/> and finds the nearest point where it crosses the polygon's boundary.
        /// </summary>
        /// <param name="vertices">The polygon vertices, in order.</param>
        /// <param name="origin">The ray's starting point.</param>
        /// <param name="direction">The ray's direction.</param>
        /// <param name="hitPoint">The nearest boundary hit point, or <paramref name="origin"/> if no hit was found.</param>
        /// <param name="hitNormal">The outward normal of the edge that was hit, or <see cref="Vector2.up"/> if no hit was found.</param>
        /// <returns><see langword="true"/> if the ray hit the boundary; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetBoundaryHit(
            this IReadOnlyList<Vector2> vertices,
            Vector2 origin,
            Vector2 direction,
            out Vector2 hitPoint,
            out Vector2 hitNormal)
        {
            var nearestDistance = float.MaxValue;
            hitPoint = origin;
            hitNormal = Vector2.up;

            for (var i = 0; i < vertices.Count; i++)
            {
                var edgeStart = vertices[i];
                var edge = vertices[(i + 1) % vertices.Count] - edgeStart;
                var denominator = direction.Cross(edge);
                if (Mathf.Abs(denominator) < DenominatorEpsilon)
                    continue;

                var toEdgeStart = edgeStart - origin;
                var rayDistance = toEdgeStart.Cross(edge) / denominator;
                var edgeFraction = toEdgeStart.Cross(direction) / denominator;
                if (rayDistance <= MinRayDistance || edgeFraction < 0f || edgeFraction > 1f ||
                    rayDistance >= nearestDistance)
                    continue;

                nearestDistance = rayDistance;
                hitPoint = origin + direction * rayDistance;
                hitNormal = new Vector2(-edge.y, edge.x).normalized;
            }

            return nearestDistance < float.MaxValue;
        }
    }
}