#ifndef PROCEDURAL_IMAGE_GLOW_INCLUDED
#define PROCEDURAL_IMAGE_GLOW_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Shared/Common.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Shared/SDF.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _Color;
CBUFFER_END

float4 _ClipRect;
int _UIVertexColorAlwaysGammaSpace;

struct Attributes
{
    float4 positionOS : POSITION;
    float4 color : COLOR;
    float2 cornerBottomLeft : TEXCOORD0;
    float2 cornerTopLeft : TEXCOORD1;
    float2 cornerTopRight : TEXCOORD2;
    float2 cornerBottomRight : TEXCOORD3;
    float3 normalOS : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float4 color : COLOR;
    float4 worldPosition : TEXCOORD0;
    float2 cornerBottomLeft : TEXCOORD1;
    float2 cornerTopLeft : TEXCOORD2;
    float2 cornerTopRight : TEXCOORD3;
    float2 cornerBottomRight : TEXCOORD4;
    float glowRadius : TEXCOORD5;
    float glowFalloffExponent : TEXCOORD6;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings Vertex(Attributes input)
{
    Varyings output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.worldPosition = input.positionOS;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

    output.cornerBottomLeft = input.cornerBottomLeft;
    output.cornerTopLeft = input.cornerTopLeft;
    output.cornerTopRight = input.cornerTopRight;
    output.cornerBottomRight = input.cornerBottomRight;
    output.glowRadius = input.normalOS.x;
    output.glowFalloffExponent = input.normalOS.y;

    #ifndef UNITY_COLORSPACE_GAMMA
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    output.color = input.color * _Color;
    return output;
}

float4 Fragment(Varyings input) : SV_Target
{
    half4 color = input.color;

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
    #endif

    float signedDistance = -SdfQuad(
        input.worldPosition.xy,
        input.cornerBottomLeft,
        input.cornerTopLeft,
        input.cornerTopRight,
        input.cornerBottomRight,
        0.0f);

    float normalizedDistance = input.glowRadius > 0.0f
        ? signedDistance / input.glowRadius
        : 0.0f;

    float glow = input.glowRadius > 0.0f
        ? saturate(1.0f / (1.0f + exp(-normalizedDistance * input.glowFalloffExponent)))
        : 0.0f;

    color.a *= glow;

    clip(color.a - 0.001f);

    return color;
}

#endif