#ifndef DEFINE
#define DEFINE

#include "Packages/com.firsttry.customutils/Shaders/Shared/Common.hlsl"

float SdfRect(float2 position, float2 halfSize)
{
    float2 distance = abs(position) - halfSize;
    float outsideNearestDistance = length(max(distance, 0.0));
    float interiorDistance = min(max(distance.x, distance.y), 0.0);
    return outsideNearestDistance + interiorDistance;
}

float SdfRoundedRect(float2 position, float2 halfSize, float4 radii)
{
    float cornerRadius = position.x > 0.0f
                             ? position.y > 0.0f
                                   ? radii.y
                                   : radii.z
                             : position.y > 0.0f
                             ? radii.x
                             : radii.w;

    float rectangleSdf = SdfRect(position, halfSize - cornerRadius);
    return rectangleSdf - cornerRadius;
}

float SdfTriangle(float2 position, float2 p0, float2 p1, float2 p2, float radius)
{
    float minEdgeDist = min(SdfSegment(position, p1, p0, 0.0),
                            min(SdfSegment(position, p1, p2, 0.0),
                                SdfSegment(position, p2, p0, 0.0)));

    float3 barycentric = mul(
        float3(position.x, position.y, 1.0),
        Inverse3X3(float3x3(
            p0.x, p0.y, 1.0,
            p1.x, p1.y, 1.0,
            p2.x, p2.y, 1.0)));

    bool isInside = barycentric.x >= 0.0 && barycentric.y >= 0.0 && barycentric.z >= 0.0;
    return isInside ? -(minEdgeDist + radius) : minEdgeDist - radius;
}

float SdfQuad(float2 position, float2 cornerBottomLeft, float2 cornerTopLeft, float2 cornerTopRight,
              float2 cornerBottomRight, float radius)
{
    float minEdgeDist = min(SdfSegment(position, cornerBottomLeft, cornerTopLeft, 0.0),
                            min(SdfSegment(position, cornerTopLeft, cornerTopRight, 0.0),
                                min(SdfSegment(position, cornerTopRight, cornerBottomRight, 0.0),
                                    SdfSegment(position, cornerBottomRight, cornerBottomLeft, 0.0))));

    float2 verts[4] = {cornerBottomLeft, cornerTopLeft, cornerTopRight, cornerBottomRight};

    for (int i = 0; i < 4; ++i)
    {
        float2 v0 = verts[i];
        float2 v1 = verts[i + 1 & 3];
        float2 v2 = verts[i + 2 & 3];
        float2 v3 = verts[i + 3 & 3];

        float2 roots = IntersectSegments(v1, v2, v3, v0);

        if (roots.x >= 0.0 && roots.x <= 1.0)
        {
            float2 intersection = v1 + (v2 - v1) * roots.x;
            float insideDist = min(SdfTriangle(position, v0, v1, intersection, 0.0),
                                   SdfTriangle(position, v3, v2, intersection, 0.0));
            float sign = insideDist <= INSIDE_THRESHOLD ? -1.0 : 1.0;
            return sign * minEdgeDist - radius;
        }

        if (roots.y >= 0.0 && roots.y <= 1.0)
        {
            float2 intersection = v3 + (v0 - v3) * roots.y;
            float insideDist = min(SdfTriangle(position, v0, v1, intersection, 0.0),
                                   SdfTriangle(position, v3, v2, intersection, 0.0));
            float sign = insideDist <= INSIDE_THRESHOLD ? -1.0 : 1.0;
            return sign * minEdgeDist - radius;
        }
    }

    // Convex quad: split into two triangles
    float insideDist = min(SdfTriangle(position, cornerBottomLeft, cornerTopLeft, cornerTopRight, 0.0),
                           SdfTriangle(position, cornerTopRight, cornerBottomRight, cornerBottomLeft, 0.0));
    float sign = insideDist <= INSIDE_THRESHOLD ? -1.0 : 1.0;
    return sign * minEdgeDist - radius;
}

#endif
