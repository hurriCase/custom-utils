#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED

float SdfRoundedRect(float2 position, float2 halfSize, float4 radii)
{
    float cornerRadius = position.x > 0.0f
                             ? position.y > 0.0f ? radii.y: radii.z: position.y > 0.0f
                             ? radii.x: radii.w;

    float2 distanceToCornerCenter = abs(position) - halfSize + cornerRadius;
    float distanceToRoundedCorner = length(max(distanceToCornerCenter, 0.0f));
    float distanceToStraightEdge = min(max(distanceToCornerCenter.x, distanceToCornerCenter.y), 0.0f);
    return cornerRadius - distanceToRoundedCorner - distanceToStraightEdge;
}

#endif
