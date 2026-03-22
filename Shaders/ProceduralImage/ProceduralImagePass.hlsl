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
    float2 uv1 : TEXCOORD1;
    float2 uv2 : TEXCOORD2;
    float2 uv3 : TEXCOORD3;
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
};

Varyings Vertex(Attributes input)
{
    Varyings OUT;

    OUT.worldPosition = input.positionOS;
    OUT.positionHCS = TransformObjectToHClip(OUT.worldPosition.xyz);

    OUT.size = input.uv1;
    OUT.texcoord = input.uv0;

    float minSide = min(OUT.size.x, OUT.size.y);

    OUT.lineWeight = input.uv3.x * minSide;
    OUT.radius = float4(Decode2(input.uv2.x), Decode2(input.uv2.y)) * minSide;
    OUT.pixelWorldScale = clamp(input.uv3.y, MIN_PIXEL_WORLD_SCALE, MAX_PIXEL_WORLD_SCALE);

    #ifndef UNITY_COLORSPACE_GAMMA
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    OUT.color = input.color * _Color;
    return OUT;
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
