#ifndef HALFTONE_OVERLAY_INCLUDED
#define HALFTONE_OVERLAY_INCLUDED

TEXTURE2D(_HalftoneTex);
SAMPLER(sampler_HalftoneTex);

// Values of CustomUtils.Runtime.UI.Halftone.HalftoneBlendMode.
#define HALFTONE_BLEND_NORMAL 0
#define HALFTONE_BLEND_DARKEN 1
#define HALFTONE_BLEND_MULTIPLY 2
#define HALFTONE_BLEND_COLOR_BURN 3
#define HALFTONE_BLEND_LINEAR_BURN 4
#define HALFTONE_BLEND_LIGHTEN 5
#define HALFTONE_BLEND_SCREEN 6
#define HALFTONE_BLEND_COLOR_DODGE 7
#define HALFTONE_BLEND_LINEAR_DODGE 8
#define HALFTONE_BLEND_OVERLAY 9
#define HALFTONE_BLEND_SOFT_LIGHT 10
#define HALFTONE_BLEND_HARD_LIGHT 11
#define HALFTONE_BLEND_DIFFERENCE 12
#define HALFTONE_BLEND_EXCLUSION 13
#define HALFTONE_BLEND_HUE 14
#define HALFTONE_BLEND_SATURATION 15
#define HALFTONE_BLEND_COLOR 16
#define HALFTONE_BLEND_LUMINOSITY 17

static const float HalftoneEpsilon = 1e-4;
static const float HalftoneMidGray = 0.5;
static const float HalftoneSoftLightDarkThreshold = 0.25;
static const float3 HalftoneLuminanceWeights = float3(0.3, 0.59, 0.11);
static const float2 HalftoneUVCenter = float2(0.5, 0.5);

// Blend formulas follow the W3C Compositing spec, which Figma implements.
float3 HalftoneBlendMultiply(float3 baseColor, float3 patternColor)
{
    return baseColor * patternColor;
}

float3 HalftoneBlendScreen(float3 baseColor, float3 patternColor)
{
    return baseColor + patternColor - baseColor * patternColor;
}

float3 HalftoneBlendHardLight(float3 baseColor, float3 patternColor)
{
    float3 isLighterThanMidGray = step(HalftoneMidGray, patternColor);
    float3 multiplied = HalftoneBlendMultiply(baseColor, 2.0 * patternColor);
    float3 screened = HalftoneBlendScreen(baseColor, 2.0 * patternColor - 1.0);
    return lerp(multiplied, screened, isLighterThanMidGray);
}

float3 HalftoneBlendColorDodge(float3 baseColor, float3 patternColor)
{
    float3 result = min(1.0, baseColor / max(1.0 - patternColor, HalftoneEpsilon));
    result = lerp(result, 1.0, step(1.0, patternColor));
    return lerp(result, 0.0, step(baseColor, 0.0));
}

float3 HalftoneBlendColorBurn(float3 baseColor, float3 patternColor)
{
    float3 result = 1.0 - min(1.0, (1.0 - baseColor) / max(patternColor, HalftoneEpsilon));
    result = lerp(result, 0.0, step(patternColor, 0.0));
    return lerp(result, 1.0, step(1.0, baseColor));
}

float3 HalftoneBlendSoftLight(float3 baseColor, float3 patternColor)
{
    float3 darkBaseCurve = ((16.0 * baseColor - 12.0) * baseColor + 4.0) * baseColor;
    float3 isDarkBase = step(baseColor, HalftoneSoftLightDarkThreshold);
    float3 softLightCurve = lerp(sqrt(baseColor), darkBaseCurve, isDarkBase);
    float3 darkened = baseColor - (1.0 - 2.0 * patternColor) * baseColor * (1.0 - baseColor);
    float3 lightened = baseColor + (2.0 * patternColor - 1.0) * (softLightCurve - baseColor);
    float3 isLighterThanMidGray = step(HalftoneMidGray, patternColor);
    return lerp(darkened, lightened, isLighterThanMidGray);
}

float HalftoneLuminance(float3 color)
{
    return dot(color, HalftoneLuminanceWeights);
}

float HalftoneSaturation(float3 color)
{
    return max(color.r, max(color.g, color.b)) - min(color.r, min(color.g, color.b));
}

float3 HalftoneClipColor(float3 color)
{
    float luminance = HalftoneLuminance(color);
    float minChannel = min(color.r, min(color.g, color.b));
    float maxChannel = max(color.r, max(color.g, color.b));
    color = minChannel < 0.0
        ? luminance + (color - luminance) * luminance / max(luminance - minChannel, HalftoneEpsilon)
        : color;
    color = maxChannel > 1.0
        ? luminance + (color - luminance) * (1.0 - luminance) / max(maxChannel - luminance, HalftoneEpsilon)
        : color;
    return color;
}

float3 HalftoneSetLuminance(float3 color, float luminance)
{
    float luminanceShift = luminance - HalftoneLuminance(color);
    return HalftoneClipColor(color + luminanceShift);
}

float3 HalftoneSetSaturation(float3 color, float saturation)
{
    float minChannel = min(color.r, min(color.g, color.b));
    float currentSaturation = max(HalftoneSaturation(color), HalftoneEpsilon);
    return (color - minChannel) * saturation / currentSaturation;
}

float3 HalftoneBlend(float3 baseColor, float3 patternColor, int blendMode)
{
    float baseLuminance = HalftoneLuminance(baseColor);
    float baseSaturation = HalftoneSaturation(baseColor);
    float patternLuminance = HalftoneLuminance(patternColor);
    float patternSaturation = HalftoneSaturation(patternColor);

    switch (blendMode)
    {
        case HALFTONE_BLEND_DARKEN:
            return min(baseColor, patternColor);
        case HALFTONE_BLEND_MULTIPLY:
            return HalftoneBlendMultiply(baseColor, patternColor);
        case HALFTONE_BLEND_COLOR_BURN:
            return HalftoneBlendColorBurn(baseColor, patternColor);
        case HALFTONE_BLEND_LINEAR_BURN:
            return max(baseColor + patternColor - 1.0, 0.0);
        case HALFTONE_BLEND_LIGHTEN:
            return max(baseColor, patternColor);
        case HALFTONE_BLEND_SCREEN:
            return HalftoneBlendScreen(baseColor, patternColor);
        case HALFTONE_BLEND_COLOR_DODGE:
            return HalftoneBlendColorDodge(baseColor, patternColor);
        case HALFTONE_BLEND_LINEAR_DODGE:
            return min(baseColor + patternColor, 1.0);
        // Overlay is Hard Light with the two colors swapped.
        case HALFTONE_BLEND_OVERLAY:
            return HalftoneBlendHardLight(patternColor, baseColor);
        case HALFTONE_BLEND_SOFT_LIGHT:
            return HalftoneBlendSoftLight(baseColor, patternColor);
        case HALFTONE_BLEND_HARD_LIGHT:
            return HalftoneBlendHardLight(baseColor, patternColor);
        case HALFTONE_BLEND_DIFFERENCE:
            return abs(baseColor - patternColor);
        case HALFTONE_BLEND_EXCLUSION:
            return baseColor + patternColor - 2.0 * baseColor * patternColor;
        case HALFTONE_BLEND_HUE:
            return HalftoneSetLuminance(HalftoneSetSaturation(patternColor, baseSaturation), baseLuminance);
        case HALFTONE_BLEND_SATURATION:
            return HalftoneSetLuminance(HalftoneSetSaturation(baseColor, patternSaturation), baseLuminance);
        case HALFTONE_BLEND_COLOR:
            return HalftoneSetLuminance(patternColor, baseLuminance);
        case HALFTONE_BLEND_LUMINOSITY:
            return HalftoneSetLuminance(baseColor, patternLuminance);
        default:
            return patternColor;
    }
}

// Figma blends in sRGB, so in a Linear project the math runs in gamma space.
float3 HalftoneToBlendSpace(float3 color)
{
    #ifdef UNITY_COLORSPACE_GAMMA
    return saturate(color);
    #else
    return saturate(LinearToSRGB(color));
    #endif
}

float3 HalftoneFromBlendSpace(float3 color)
{
    #ifdef UNITY_COLORSPACE_GAMMA
    return color;
    #else
    return SRGBToLinear(color);
    #endif
}

half4 ApplyHalftone(half4 baseColor, float2 uv)
{
    float2 centeredUV = uv - HalftoneUVCenter;
    float rotationSin, rotationCos;
    float rotationRadians = DegToRad(_PatternRotation);
    sincos(rotationRadians, rotationSin, rotationCos);
    float2 rotatedUV = float2(
        rotationCos * centeredUV.x - rotationSin * centeredUV.y,
        rotationSin * centeredUV.x + rotationCos * centeredUV.y
    );
    float2 patternUV = (rotatedUV + HalftoneUVCenter) * _PatternScale.xy + _PatternOffset.xy;

    float2 withinBounds = step(0.0, patternUV) * step(patternUV, 1.0);
    float insidePattern = withinBounds.x * withinBounds.y;

    half4 pattern = SAMPLE_TEXTURE2D(_HalftoneTex, sampler_HalftoneTex, patternUV);

    float patternAlpha = pattern.a * insidePattern * _PatternOpacity;
    float3 baseColorBlend = HalftoneToBlendSpace(baseColor.rgb);
    float3 patternColorBlend = HalftoneToBlendSpace(pattern.rgb);
    float3 blendedColor = saturate(HalftoneBlend(baseColorBlend, patternColorBlend, (int)_BlendMode));
    float3 finalColor = lerp(baseColorBlend, blendedColor, patternAlpha);

    return half4(HalftoneFromBlendSpace(finalColor), baseColor.a);
}

#endif