#ifndef THEME_PASS_INCLUDED
#define THEME_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Shared/Common.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
float4 _Color;
float4 _PatternOffset;
float4 _PatternScale;
float _PatternOpacity;
float _BlendMode;
float _PatternRotation;
float _AspectRatio;
CBUFFER_END

#include "Packages/com.firsttry.customutils/Shaders/Halftone/HalftoneUtils.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Theme/GradientUtils.hlsl"

float4 _ClipRect;
int _UIVertexColorAlwaysGammaSpace;

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    float4 themeData : TEXCOORD2;
    float4 gradientSettings : TEXCOORD3;
    float4 color : COLOR;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 worldPosition : TEXCOORD1;
    float4 themeData : TEXCOORD2;
    float4 gradientSettings : TEXCOORD3;
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

    output.worldPosition = input.positionOS;
    output.uv = input.uv;
    output.themeData = input.themeData;
    output.gradientSettings = input.gradientSettings;
    output.color = input.color * _Color;
    return output;
}

float4 Fragment(Varyings input) : SV_Target
{
    half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

    #ifdef THEME_GRADIENT
    color *= SampleThemeGradient(input.themeData, input.gradientSettings);
    #endif

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001f);
    #endif

    #ifdef HALFTONE_ON
    color = ApplyHalftone(color, input.themeData.xy);
    #endif

    return color;
}

#endif
