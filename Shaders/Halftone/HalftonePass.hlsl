#ifndef HALFTONE_INCLUDED
#define HALFTONE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
float4 _Color;
float4 _PatternOffset;
float4 _PatternScale;
float  _PatternOpacity;
float4 _DotColor;
float  _PatternRotation;
CBUFFER_END

#include "HalftoneUtils.hlsl"

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

    #if UNITY_COLORSPACE_GAMMA == 0
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    output.uv = input.uv;
    output.color = input.color * _Color;
    return output;
}

float4 Fragment(Varyings input) : SV_Target
{
    half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001f);
    #endif

    return ApplyHalftone(color, input.uv);
}

#endif
