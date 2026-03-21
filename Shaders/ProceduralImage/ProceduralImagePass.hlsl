#ifndef PROCEDURALIMAGE_INCLUDED
#define PROCEDURALIMAGE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Common.hlsl"

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
    float4 vertex : SV_POSITION;
    half4 color : COLOR;
    float4 worldPosition : TEXCOORD0;
    float4 radius : TEXCOORD1;
    float2 texcoord : TEXCOORD2;
    float2 size : TEXCOORD3;
    float lineWeight : TEXCOORD4;
    float pixelWorldScale : TEXCOORD5;
};

CBUFFER_START(UnityPerMaterial)
half4 _Color;
CBUFFER_END

float4 _ClipRect;
int _UIVertexColorAlwaysGammaSpace;

static const float Max16BitValue = 65535.0f;
static const float MaxPixelWorldScale = 2048.0f;
static const float MinPixelWorldScale = 1.0f / MaxPixelWorldScale;

float2 Decode2(float value)
{
    float2 encodeMul = float2(1.0f, Max16BitValue);
    float encodeBit = 1.0f / Max16BitValue;
    float2 enc = encodeMul * value;
    enc = frac(enc);
    enc.x -= enc.y * encodeBit;
    return enc;
}

Varyings Vertex(Attributes input)
{
    Varyings OUT;

    OUT.worldPosition = input.positionOS;
    OUT.vertex = TransformObjectToHClip(OUT.worldPosition.xyz);

    OUT.size = input.uv1;
    OUT.texcoord = input.uv0;

    float minSide = min(OUT.size.x, OUT.size.y);

    OUT.lineWeight = input.uv3.x * minSide;
    OUT.radius = float4(Decode2(input.uv2.x), Decode2(input.uv2.y)) * minSide;
    OUT.pixelWorldScale = clamp(input.uv3.y, MinPixelWorldScale, MaxPixelWorldScale);

    #ifndef UNITY_COLORSPACE_GAMMA
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    OUT.color = input.color * _Color;
    return OUT;
}

half4 Fragment(Varyings IN) : SV_Target
{
    half4 color = IN.color;

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001f);
    #endif

    float2 halfSize = IN.size * 0.5f;
    float2 centeredPosition = IN.texcoord * IN.size - halfSize;
    float sdf = SdfRoundedRect(centeredPosition, halfSize, IN.radius);
    float borderCenter = (IN.lineWeight + 1.0f / IN.pixelWorldScale) * 0.5f;
    color.a *= saturate((borderCenter - abs(sdf - borderCenter)) * IN.pixelWorldScale);

    if (color.a <= 0)
        discard;

    return color;
}

#endif
