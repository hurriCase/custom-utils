#ifndef PROCEDURAL_IMAGE_INCLUDED
#define PROCEDURAL_IMAGE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Shared/Common.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Shared/QuadSDF.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float4 _Color;
    float4 _MainTex_ST;
    float4 _TextureSampleAdd;
CBUFFER_END

float4 _ClipRect;
int _UIVertexColorAlwaysGammaSpace;

struct Attributes
{
    float4 positionOS : POSITION;
    float4 color : COLOR;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float4 color : COLOR;
    float4 worldPosition : TEXCOORD0;
    float4 uvAndBorder : TEXCOORD1;
    float4 edges : TEXCOORD2;
    float4 radii : TEXCOORD3;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings Vertex(Attributes input)
{
    Varyings output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.worldPosition = input.positionOS;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uvAndBorder.xy = TRANSFORM_TEX(input.uv0.xy, _MainTex);
    output.uvAndBorder.z = input.uv0.z;
    output.uvAndBorder.w = clamp(input.uv0.w, MIN_PIXEL_WORLD_SCALE, MAX_PIXEL_WORLD_SCALE);
    output.edges = input.uv1;
    output.radii = input.uv2;

    #ifndef UNITY_COLORSPACE_GAMMA
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    output.color = input.color * _Color;
    return output;
}

float4 Fragment(Varyings input) : SV_Target
{
    half4 color = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvAndBorder.xy) + _TextureSampleAdd) * input.color;

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001f);
    #endif

    float lineWeight = input.uvAndBorder.z;
    float pixelScale = input.uvAndBorder.w;

    float depth = -SdfRoundedQuad(input.edges, input.radii);

    float fill = saturate(depth * pixelScale);
    float borderCenter = (lineWeight + 1.0f / pixelScale) * 0.5f;
    float border = saturate((borderCenter - distance(depth, borderCenter)) * pixelScale);
    color.a *= lineWeight > 0.0f ? border : fill;

    clip(color.a - 0.001f);

    return color;
}

#endif
