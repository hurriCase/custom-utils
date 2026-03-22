#ifndef PROCEDURALIMAGE_INCLUDED
#define PROCEDURALIMAGE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Common.hlsl"

CBUFFER_START(UnityPerMaterial)
half4 _Color;
CBUFFER_END

float4 _ClipRect;
int _UIVertexColorAlwaysGammaSpace;

struct Attributes
{
    float4 positionOS : POSITION;
    float4 color : COLOR;
    float2 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    float4 uv3 : TEXCOORD3;
    float4 tangent : TANGENT;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    half4 color : COLOR;
    float4 worldPosition : TEXCOORD0;
    float4 radius : TEXCOORD1;
    float2 texcoord : TEXCOORD2;
    float2 size : TEXCOORD3;
    float lineWeight : TEXCOORD4;
    float pixelWorldScale : TEXCOORD5;
    float2 cornerOffsetTopLeft : TEXCOORD6;
    float2 cornerOffsetTopRight : TEXCOORD7;
    float2 cornerOffsetBottomRight : TEXCOORD8;
    float2 cornerOffsetBottomLeft : TEXCOORD9;
};

Varyings Vertex(Attributes input)
{
    Varyings output;

    output.worldPosition = input.positionOS;
    output.positionHCS = TransformObjectToHClip(output.worldPosition.xyz);

    output.size = input.uv1;
    output.texcoord = input.uv0;

    float minSide = min(output.size.x, output.size.y);

    output.lineWeight = input.uv3.x * minSide;
    output.radius = float4(Decode2(input.uv2.x), Decode2(input.uv2.y)) * minSide;
    output.pixelWorldScale = clamp(input.uv3.y, MIN_PIXEL_WORLD_SCALE, MAX_PIXEL_WORLD_SCALE);

    #ifndef UNITY_COLORSPACE_GAMMA
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    output.size = input.uv1.xy;
    output.cornerOffsetTopLeft = input.uv1.zw;
    output.cornerOffsetTopRight = input.uv2.zw;
    output.cornerOffsetBottomRight = input.uv3.zw;
    output.cornerOffsetBottomLeft = input.tangent.xy;

    output.color = input.color * _Color;
    return output;
}

half4 Fragment(Varyings input) : SV_Target
{
    half4 color = input.color;

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(input.positionHCS.xy, _ClipRect);
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001f);
    #endif

    float2 halfSize = input.size * 0.5f;
    float2 centeredPosition = input.texcoord * input.size - halfSize;
    float sdf = SdfRoundedRect(centeredPosition, halfSize, input.radius);
    float borderCenter = (input.lineWeight + 1.0f / input.pixelWorldScale) * 0.5f;
    color.a *= saturate((borderCenter - abs(sdf - borderCenter)) * input.pixelWorldScale);

    #ifdef HALFTONE_ON
    color = ApplyHalftone(color, input.uv);
    #endif

    if (color.a <= 0)
        discard;

    return color;
}

#endif
