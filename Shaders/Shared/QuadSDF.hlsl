#ifndef QUAD_SDF_INCLUDED
#define QUAD_SDF_INCLUDED

#include "Packages/com.firsttry.customutils/Shaders/Shared/Common.hlsl"

#define MIN_EDGE_LENGTH 0.001f

struct QuadEdges
{
    float top;
    float right;
    float bottom;
    float left;
};

float DistanceToEdge(float2 a, float2 b, float orientation)
{
    float2 edge = b - a;
    float2 direction = edge / max(length(edge), MIN_EDGE_LENGTH);
    float2 outward = float2(-direction.y, direction.x) * orientation;
    return -dot(a, outward);
}

QuadEdges CalculateQuadEdges(float2 topLeft, float2 topRight, float2 bottomRight, float2 bottomLeft)
{
    float doubleArea = Cross2D(topLeft, topRight) + Cross2D(topRight, bottomRight)
        + Cross2D(bottomRight, bottomLeft) + Cross2D(bottomLeft, topLeft);
    float orientation = doubleArea < 0.0f ? 1.0f : -1.0f;

    QuadEdges edges;
    edges.top = DistanceToEdge(topLeft, topRight, orientation);
    edges.right = DistanceToEdge(topRight, bottomRight, orientation);
    edges.bottom = DistanceToEdge(bottomRight, bottomLeft, orientation);
    edges.left = DistanceToEdge(bottomLeft, topLeft, orientation);
    return edges;
}

float SdfRoundedQuad(float4 edges, float4 radii)
{
    bool isRight = edges.y > edges.w;
    bool isTop = edges.x > edges.z;
    float radius = isRight ? isTop ? radii.y : radii.z : isTop ? radii.x : radii.w;

    float2 q = float2(max(edges.w, edges.y), max(edges.x, edges.z)) + radius;
    return length(max(q, 0.0f)) + min(max(q.x, q.y), 0.0f) - radius;
}

float SdfRoundedQuad(QuadEdges edges, float4 radii)
{
    return SdfRoundedQuad(float4(edges.top, edges.right, edges.bottom, edges.left), radii);
}

#endif
