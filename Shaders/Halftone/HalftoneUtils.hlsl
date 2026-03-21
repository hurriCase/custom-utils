#ifndef HALFTONE_OVERLAY_INCLUDED
#define HALFTONE_OVERLAY_INCLUDED

TEXTURE2D(_HalftoneTex);
SAMPLER(sampler_HalftoneTex);

half4 ApplyHalftone(half4 baseColor, float2 uv)
{
    float2 centeredUV = uv - 0.5;
    float sinR, cosR;
    float rotationRadians = DegToRad(_PatternRotation);
    sincos(rotationRadians, sinR, cosR);
    float2 rotatedUV = float2(
        cosR * centeredUV.x - sinR * centeredUV.y,
        sinR * centeredUV.x + cosR * centeredUV.y
    );
    float2 patternUV = (rotatedUV + 0.5) * _PatternScale.xy + _PatternOffset.xy;

    float2 withinBounds = step(0.0, patternUV) * step(patternUV, 1.0);
    float insidePattern = withinBounds.x * withinBounds.y;

    half4 pattern = SAMPLE_TEXTURE2D(_HalftoneTex, sampler_HalftoneTex, patternUV);

    half darkness = (1.0 - pattern.r) * insidePattern;
    half3 finalColor = lerp(baseColor.rgb, _DotColor.rgb, darkness * _PatternOpacity);

    return half4(finalColor, baseColor.a);
}

#endif