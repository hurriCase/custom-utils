#ifndef TEXT_THEME_PASS_INCLUDED
#define TEXT_THEME_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Shared/Common.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
float4 _FaceColor;
float _FaceDilate;
float4 _OutlineColor;
float _OutlineWidth;
float _OutlineSoftness;
float4 _UnderlayColor;
float _UnderlayOffsetX;
float _UnderlayOffsetY;
float _UnderlayDilate;
float _UnderlaySoftness;
float _TextureWidth;
float _TextureHeight;
float _GradientScale;
float _ScaleX;
float _ScaleY;
float _PerspectiveFilter;
float _Sharpness;
float _WeightNormal;
float _WeightBold;
float _ScaleRatioA;
float _ScaleRatioC;
float _VertexOffsetX;
float _VertexOffsetY;
float _MaskSoftnessX;
float _MaskSoftnessY;
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
float _UIMaskSoftnessX;
float _UIMaskSoftnessY;
int _UIVertexColorAlwaysGammaSpace;

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 color : COLOR;
    // xy: atlas position, w: glyph scale, negative for bold glyphs
    float4 atlasData : TEXCOORD0;
    float4 themeData : TEXCOORD2;
    float4 gradientSettings : TEXCOORD3;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float4 color : COLOR;
    float2 atlasPosition : TEXCOORD0;
    // x: alpha clip, y: distance scale, z: distance bias
    float3 distanceSettings : TEXCOORD1;
    // xy: position relative to the clip rect, zw: clip softness
    float4 clipData : TEXCOORD2;
    float4 themeData : TEXCOORD3;
    float4 gradientSettings : TEXCOORD4;
    #ifdef UNDERLAY_ON
    // xy: atlas position, z: distance scale, w: distance bias
    float4 underlayData : TEXCOORD5;
    float4 underlayColor : TEXCOORD6;
    #endif
};

float ComputeDistanceScale(float4 position, float3 normal, float4 positionHCS, float glyphScale, out float2 pixelSize)
{
    pixelSize = positionHCS.w;
    pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

    float scale = rsqrt(dot(pixelSize, pixelSize));
    scale *= abs(glyphScale) * _GradientScale * (_Sharpness + 1);

    bool isPerspective = UNITY_MATRIX_P[3][3] == 0;
    if (!isPerspective)
        return scale;

    float3 worldNormal = TransformObjectToWorldNormal(normal);
    float3 viewDirection = normalize(GetWorldSpaceViewDir(TransformObjectToWorld(position.xyz)));
    float facing = abs(dot(worldNormal, viewDirection));
    return lerp(abs(scale) * (1 - _PerspectiveFilter), scale, facing);
}

Varyings Vertex(Attributes input)
{
    Varyings output;

    float4 position = input.positionOS;
    position.xy += float2(_VertexOffsetX, _VertexOffsetY);
    output.positionHCS = TransformObjectToHClip(position.xyz);

    #ifndef UNITY_COLORSPACE_GAMMA
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    float glyphScale = input.atlasData.w;
    float2 pixelSize;
    float scale = ComputeDistanceScale(position, input.normalOS, output.positionHCS, glyphScale, pixelSize);

    float isBold = step(glyphScale, 0);
    float weight = lerp(_WeightNormal, _WeightBold, isBold) / 4.0f;
    weight = (weight + _FaceDilate) * _ScaleRatioA * 0.5f;

    float bias = 0.5f - weight + 0.5f / scale;
    float alphaClip = 1.0f - _OutlineWidth * _ScaleRatioA - _OutlineSoftness * _ScaleRatioA;
    alphaClip = alphaClip / 2.0f - 0.5f / scale - weight;

    #ifdef UNDERLAY_ON
    float underlayScale = scale / (1 + _UnderlaySoftness * _ScaleRatioC * scale);
    float underlayDilate = _UnderlayDilate * _ScaleRatioC * 0.5f;
    float underlayBias = (0.5f - weight - underlayDilate) * underlayScale - 0.5f;
    float2 underlayOffset = float2(_UnderlayOffsetX / _TextureWidth, _UnderlayOffsetY / _TextureHeight);
    underlayOffset *= -_ScaleRatioC * _GradientScale;

    output.underlayData = float4(input.atlasData.xy + underlayOffset, underlayScale, underlayBias);
    output.underlayColor = float4(_UnderlayColor.rgb * _UnderlayColor.a, _UnderlayColor.a);
    #endif

    float4 clampedClipRect = clamp(_ClipRect, -2e10, 2e10);
    float2 maskSoftness = float2(max(_UIMaskSoftnessX, _MaskSoftnessX), max(_UIMaskSoftnessY, _MaskSoftnessY));

    output.color = input.color;
    output.atlasPosition = input.atlasData.xy;
    output.distanceSettings = float3(alphaClip, scale, bias);
    output.clipData = float4(
        position.xy * 2 - clampedClipRect.xy - clampedClipRect.zw,
        0.25f / (0.25f * maskSoftness + pixelSize));
    output.themeData = input.themeData;
    output.gradientSettings = input.gradientSettings;
    return output;
}

half4 ComposeFaceAndOutline(float signedDistance, half4 faceColor, half4 outlineColor, float outline, float softness)
{
    float faceAlpha = 1 - saturate((signedDistance - outline * 0.5f + softness * 0.5f) / (1.0f + softness));
    float outlineAlpha = saturate(signedDistance + outline * 0.5f) * sqrt(min(1.0f, outline));

    faceColor.rgb *= faceColor.a;
    outlineColor.rgb *= outlineColor.a;

    return lerp(faceColor, outlineColor, outlineAlpha) * faceAlpha;
}

float4 Fragment(Varyings input) : SV_Target
{
    float alphaClip = input.distanceSettings.x;
    float scale = input.distanceSettings.y;
    float bias = input.distanceSettings.z;

    float glyphDistance = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.atlasPosition).a;

    #ifndef UNDERLAY_ON
    clip(glyphDistance - alphaClip);
    #endif

    float signedDistance = (bias - glyphDistance) * scale;
    float outline = _OutlineWidth * _ScaleRatioA * scale;
    float softness = _OutlineSoftness * _ScaleRatioA * scale;

    half4 faceColor = _FaceColor;
    faceColor.rgb *= input.color.rgb;

    #ifdef THEME_GRADIENT
    faceColor *= SampleThemeGradient(input.themeData, input.gradientSettings);
    #endif

    // Only the face gets the pattern, like a Figma image fill on text.
    #ifdef HALFTONE_ON
    faceColor = ApplyHalftone(faceColor, input.themeData.xy);
    #endif

    half4 color = ComposeFaceAndOutline(signedDistance, faceColor, _OutlineColor, outline, softness);

    #ifdef UNDERLAY_ON
    float underlayDistance = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.underlayData.xy).a;
    underlayDistance *= input.underlayData.z;
    color += input.underlayColor * saturate(underlayDistance - input.underlayData.w) * (1 - color.a);
    #endif

    #ifdef UNITY_UI_CLIP_RECT
    float2 clipFactor = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.clipData.xy)) * input.clipData.zw);
    color *= clipFactor.x * clipFactor.y;
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a - 0.001f);
    #endif

    return color * input.color.a;
}

#endif
