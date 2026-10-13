#ifndef GRADIENT_UTILS_INCLUDED
#define GRADIENT_UTILS_INCLUDED

// Values of CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient.GradientType.
#define GRADIENT_TYPE_LINEAR 0
#define GRADIENT_TYPE_DIAMOND 1
#define GRADIENT_TYPE_RADIAL 2
#define GRADIENT_TYPE_ANGULAR 3

TEXTURE2D(_GradientAtlas);
SAMPLER(sampler_GradientAtlas);
float _GradientAtlasHeight;

float ComputeGradientTime(float2 centerPoint, int gradientType, float rotation, int diamondDirection)
{
    switch (gradientType)
    {
        case GRADIENT_TYPE_DIAMOND:
        {
            float chebyshevDistance = max(abs(centerPoint.x), abs(centerPoint.y));
            // multiply by 2 to remap chebyshev distance from 0..0.5 to 0..1
            float distanceToEdge = saturate(chebyshevDistance * 2.0f);
            return abs(diamondDirection - distanceToEdge);
        }
        case GRADIENT_TYPE_RADIAL:
            // multiply by 2 to remap Euclidean distance from 0..0.5 to 0..1
            return saturate(length(centerPoint) * 2.0f);
        case GRADIENT_TYPE_ANGULAR:
            // atan2 returns -PI..PI, remap to 0..1 and rotate
            return frac(atan2(centerPoint.x, centerPoint.y) / TWO_PI + 0.5f + rotation / DEGREES_MAX);
        default:
            return ProjectUVOnAxis(centerPoint, rotation);
    }
}

// themeData: position within the element (xy), atlas row (z), gradient type (w).
// gradientSettings: rotation in degrees (x), diamond direction (y).
half4 SampleThemeGradient(float4 themeData, float4 gradientSettings)
{
    float2 centerPoint = themeData.xy - 0.5f;
    float atlasRow = themeData.z;
    int gradientType = (int)round(themeData.w);
    float rotation = gradientSettings.x;
    int diamondDirection = (int)round(gradientSettings.y);

    float time = ComputeGradientTime(centerPoint, gradientType, rotation, diamondDirection);
    float atlasRowCoordinate = (atlasRow + 0.5f) / _GradientAtlasHeight;
    return SAMPLE_TEXTURE2D(_GradientAtlas, sampler_GradientAtlas, float2(time, atlasRowCoordinate));
}

#endif
