#ifndef DIAMOND_INCLUDED
#define DIAMOND_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_GradientTex);
SAMPLER(sampler_GradientTex);

CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
float4 _Color;
int    _Direction;
float4 _PatternOffset;
float4 _PatternScale;
float  _PatternOpacity;
float4 _DotColor;
float  _PatternRotation;
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

    #if UNITY_COLORSPACE_GAMMA
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    output.uv = input.uv;
    output.color = input.color;
    return output;
}

float4 Fragment(Varyings input) : SV_Target
{
    float2 centeredUV = input.uv - 0.5;
    float t = saturate(max(abs(centeredUV.x), abs(centeredUV.y)) * 2.0);

    if (_Direction == 1) t = 1.0 - t;

    half4 gradientColor = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(t, 0));
    half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    half4 color = gradientColor * mainTex * input.color * _Color;

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001f);
    #endif

    #ifdef HALFTONE_ON
    color = ApplyHalftone(color, input.uv);
    #endif

    return input.color * _Color;
}

#endif
