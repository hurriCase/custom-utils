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
    float _Saturation;
CBUFFER_END

// Halftone settings come per element from the style data texture instead of material properties.
#define HALFTONE_EXTERNAL_PARAMS
#include "Packages/com.firsttry.customutils/Shaders/Halftone/HalftoneUtils.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Theme/GradientUtils.hlsl"

// Written by CustomUtils.Runtime.UI.Theme.StyleData.StyleDataTexture: one row per style, four texels per row.
TEXTURE2D(_ThemeStyleData);

#define NO_STYLE_ROW -1
#define STYLE_TEXEL_GRADIENT 0
#define STYLE_TEXEL_HALFTONE 1
#define STYLE_TEXEL_PATTERN 2
#define STYLE_TEXEL_COLOR 3

#define MIN_COVERAGE_WIDTH 1e-4f
#define MIN_ALPHA 1e-4h
#define LUMINANCE_WEIGHTS half3(0.2126h, 0.7152h, 0.0722h)

float4 _ClipRect;
int _UIVertexColorAlwaysGammaSpace;

struct Attributes
{
    float4 positionOS : POSITION;
    float4 color : COLOR;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    float4 uv3 : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float4 color : COLOR;
    float4 worldPosition : TEXCOORD0;
    // Sprite uv (xy), fill style row (z), outline style row (w).
    float4 uvAndRows : TEXCOORD1;
    float4 edges : TEXCOORD2;
    float4 radii : TEXCOORD3;
    // Ring width (x), outline width (y), outline fraction outside the edge (z), shadow style row (w).
    float4 styleParams : TEXCOORD4;
    // Position within the rect (xy) and its aspect ratio (z), for gradients and halftone.
    float3 positionAndAspect : TEXCOORD5;
    // Edge distance shift that moves the shape by the shadow offset.
    float4 shadowShift : TEXCOORD6;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings Vertex(Attributes input)
{
    Varyings output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.worldPosition = input.positionOS;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uvAndRows.xy = TRANSFORM_TEX(input.uv0.xy, _MainTex);
    output.uvAndRows.zw = input.uv0.zw;
    output.edges = input.uv1;
    output.radii = input.uv2;
    output.styleParams = input.uv3;
    output.positionAndAspect = input.normal;
    output.shadowShift = input.tangent;

    #ifndef UNITY_COLORSPACE_GAMMA
    if (_UIVertexColorAlwaysGammaSpace)
        input.color.rgb = SRGBToLinear(input.color.rgb);
    #endif

    output.color = input.color * _Color;
    return output;
}

float4 LoadStyleTexel(int row, int texel)
{
    return LOAD_TEXTURE2D(_ThemeStyleData, int2(texel, row));
}

half4 ApplyStyle(half4 color, int row, float2 position, float aspectRatio)
{
    if (row == NO_STYLE_ROW)
        return color;

    float4 gradient = LoadStyleTexel(row, STYLE_TEXEL_GRADIENT);
    float4 halftone = LoadStyleTexel(row, STYLE_TEXEL_HALFTONE);
    color *= (half4)LoadStyleTexel(row, STYLE_TEXEL_COLOR);

    if (gradient.x >= 0.0f)
        color *= SampleThemeGradient(float4(position, gradient.x, gradient.y), float4(gradient.zw, 0.0f, 0.0f));

    if (halftone.x < 0.5f)
        return color;

    float4 pattern = LoadStyleTexel(row, STYLE_TEXEL_PATTERN);
    return ApplyHalftonePattern(color, position, aspectRatio, pattern.xy, pattern.zw, halftone.y,
        (int)round(halftone.z), halftone.w);
}

// Antialiased coverage of a depth that is positive inside, one pixel wide at any scale.
float Coverage(float depth)
{
    float width = max(fwidth(depth), MIN_COVERAGE_WIDTH);
    return saturate(depth / width + 0.5f);
}

float BandCoverage(float depth, float start, float width)
{
    return Coverage(depth - start) * Coverage(start + width - depth);
}

float ShapeCoverage(float depth, float ringWidth)
{
    return ringWidth > 0.0f ? BandCoverage(depth, 0.0f, ringWidth) : Coverage(depth);
}

half4 BlendOver(half4 top, half4 bottom)
{
    half alpha = top.a + bottom.a * (1.0h - top.a);
    half3 rgb = (top.rgb * top.a + bottom.rgb * bottom.a * (1.0h - top.a)) / max(alpha, MIN_ALPHA);
    return half4(rgb, alpha);
}

float4 Fragment(Varyings input) : SV_Target
{
    float2 position = input.positionAndAspect.xy;
    float aspectRatio = input.positionAndAspect.z;
    int fillRow = (int)round(input.uvAndRows.z);
    int outlineRow = (int)round(input.uvAndRows.w);
    float ringWidth = input.styleParams.x;
    float outlineWidth = input.styleParams.y;
    float outlineOutside = input.styleParams.z;
    int shadowRow = (int)round(input.styleParams.w);

    bool hasOutline = outlineRow != NO_STYLE_ROW;
    float outset = hasOutline ? outlineWidth * outlineOutside : 0.0f;
    float inset = hasOutline ? outlineWidth - outset : 0.0f;

    // Coverages first: they use screen derivatives, which must not sit inside branches.
    float depth = -SdfRoundedQuad(input.edges, input.radii);
    float shadowDepth = -SdfRoundedQuad(input.edges - input.shadowShift, input.radii);
    // The outer edge (shape plus an outside outline) is antialiased once and the outline replaces the fill inside it,
    // so the fill never bleeds through the outline's antialiased border.
    float edgeCoverage = ShapeCoverage(depth + outset, ringWidth);
    float shadowCoverage = ShapeCoverage(shadowDepth + outset, ringWidth);
    float outlineMask = hasOutline ? Coverage(inset - depth) : 0.0f;
    float fillMask = outset > 0.0f ? Coverage(depth) : 1.0f;

    // The tint alpha carries canvas group fading, so it fades the shadow and the outline too.
    half tintAlpha = input.color.a;

    half4 fill = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvAndRows.xy) + _TextureSampleAdd) * input.color;
    fill = ApplyStyle(fill, fillRow, position, aspectRatio);
    fill.a *= fillMask;

    half4 result = fill;

    if (hasOutline)
    {
        half4 outline = ApplyStyle(half4(1.0h, 1.0h, 1.0h, 1.0h), outlineRow, position, aspectRatio);
        outline.a *= outlineMask * tintAlpha;
        result = BlendOver(outline, result);
    }

    result.a *= edgeCoverage;

    if (shadowRow != NO_STYLE_ROW)
    {
        half4 shadow = (half4)LoadStyleTexel(shadowRow, STYLE_TEXEL_COLOR);
        shadow.a *= shadowCoverage * tintAlpha;
        result = BlendOver(result, shadow);
    }

    half luminance = dot(result.rgb, LUMINANCE_WEIGHTS);
    result.rgb = lerp(half3(luminance, luminance, luminance), result.rgb, (half)_Saturation);

    #ifdef UNITY_UI_CLIP_RECT
    result.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
    #endif

    clip(result.a - 0.001f);

    return result;
}

#endif
