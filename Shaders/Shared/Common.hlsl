#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED

#define DEGREES_MAX 360.0f
#define MAX_16_BIT_VALUE 65535.0f
#define MAX_PIXEL_WORLD_SCALE 2048.0f
#define MIN_PIXEL_WORLD_SCALE 1.0f / MAX_PIXEL_WORLD_SCALE

#define INSIDE_THRESHOLD 0.001

float ProjectUVOnAxis(float2 centerPoint, float angleDegrees)
{
    float radians = DegToRad(angleDegrees);
    float2 direction = float2(cos(radians), sin(radians));
    return dot(centerPoint, direction);
}

float2 Decode2(float value)
{
    float2 encodeMul = float2(1.0f, MAX_16_BIT_VALUE);
    float encodeBit = 1.0f / MAX_16_BIT_VALUE;
    float2 enc = encodeMul * value;
    enc = frac(enc);
    enc.x -= enc.y * encodeBit;
    return enc;
}

float Cross2D(float2 left, float2 right)
{
    return determinant(float2x2(left, right));
}

float SdfSegment(float2 position, float2 segmentStart, float2 segmentEnd, float radius)
{
    float2 startToPoint = position - segmentStart;
    float2 startToEnd = segmentEnd - segmentStart;
    float t = clamp(dot(startToPoint, startToEnd) / dot(startToEnd, startToEnd), 0.0, 1.0);
    return length(startToPoint - startToEnd * t) - radius;
}

float3x3 Inverse3X3(float3x3 m)
{
    float det = determinant(m);
    float3x3 adj = float3x3(
        m[1][1] * m[2][2] - m[1][2] * m[2][1], m[0][2] * m[2][1] - m[0][1] * m[2][2],
        m[0][1] * m[1][2] - m[0][2] * m[1][1],
        m[1][2] * m[2][0] - m[1][0] * m[2][2], m[0][0] * m[2][2] - m[0][2] * m[2][0],
        m[0][2] * m[1][0] - m[0][0] * m[1][2],
        m[1][0] * m[2][1] - m[1][1] * m[2][0], m[0][1] * m[2][0] - m[0][0] * m[2][1],
        m[0][0] * m[1][1] - m[0][1] * m[1][0]
    );
    return adj / det;
}

float2 IntersectSegments(float2 p0, float2 p1, float2 p2, float2 p3)
{
    float2 dirA = p1 - p0;
    float2 dirB = p3 - p2;
    float2 originDelta = p2 - p0;
    float invCross = 1.0 / Cross2D(dirA, dirB);
    return float2(
        Cross2D(originDelta, dirB) * invCross,
        Cross2D(originDelta, dirA) * invCross);
}

#endif
