#ifndef GRADIENT_INCLUDED
#define GRADIENT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Shared/Common.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_GradientTex);
SAMPLER(sampler_GradientTex);

CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
float4 _Color;
int _Direction;
int _Rotation;
float4 _PatternOffset;
float4 _PatternScale;
float _PatternOpacity;
float4 _DotColor;
float _PatternRotation;
CBUFFER_END

#include "Packages/com.firsttry.customutils/Shaders/Halftone/HalftoneUtils.hlsl"

int _UIVertexColorAlwaysGammaSpace;

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};

Varyings Vertex(Attributes input)
{
    Varyings output;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

    #ifndef UNITY_COLORSPACE_GAMMA
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    output.uv = input.uv;
    output.color = input.color;
    return output;
}

float ComputeGradientT(float2 centerPoint)
{
    #if defined(GRADIENT_DIAMOND)
    float chebyshevDistance = max(abs(centerPoint.x), abs(centerPoint.y));
    // multiply by 2 to remap chebyshev distance from 0..0.5 to 0..1
    float sdf = saturate(chebyshevDistance * 2.0f);
    return abs(_Direction - sdf);
    #elif defined(GRADIENT_RADIAL)
    // multiply by 2 to remap Euclidean distance from 0..0.5 to 0..1
    return saturate(length(centerPoint) * 2.0f);
    #elif defined(GRADIENT_ANGULAR)
    // atan2 returns -PI..PI, remap to 0..1 and rotate
    return frac(atan2(centerPoint.x, centerPoint.y) / TWO_PI + 0.5f + _Rotation / DEGREES_MAX);
    #else
    return ProjectUVOnAxis(centerPoint, _Rotation);
    #endif
}

float4 Fragment(Varyings input) : SV_Target
{
    float2 centerPoint = input.uv.xy - 0.5f;
    float t = ComputeGradientT(centerPoint);

    half4 gradientColor = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(t, 0));
    half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    half4 color = gradientColor * mainTex * input.color * _Color;

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(input.positionHCS.xy, _ClipRect);
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001f);
    #endif

    #ifdef HALFTONE_ON
    color = ApplyHalftone(color, input.uv);
    #endif

    return color;
}

#endif
