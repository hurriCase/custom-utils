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
// Value of CustomUtils.Runtime.UI.Theme.StyleData.StyleDataTexture.PackedRowBase.
#define PACKED_ROW_BASE 256.0f

#define MIN_COVERAGE_WIDTH 1e-4f
#define MIN_ALPHA 1e-4h
#define MIN_CANVAS_DETERMINANT 1e-8f

float4 _ClipRect;
int _UIVertexColorAlwaysGammaSpace;

// Canvas batching rotates normals and tangents with the element, so only the tangent's xy holds a direction
// and everything positional comes through the uv channels.
struct Attributes
{
    float4 positionOS : POSITION;
    float4 color : COLOR;
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
    // Canvas space position.
    float4 worldPosition : TEXCOORD0;
    // Sprite uv (xy), position within the rect (zw) for gradients and halftone.
    float4 uvAndPosition : TEXCOORD1;
    float4 edges : TEXCOORD2;
    float4 radii : TEXCOORD3;
    // Ring width (x), outline width (y), outline fraction outside the edge (z).
    float3 effectParams : TEXCOORD4;
    // Fill (x), outline (y) and shadow (z) style rows.
    float3 styleRows : TEXCOORD5;
    // Shadow offset in canvas space (xy) and the rect's aspect ratio (z).
    float3 shadowOffsetAndAspect : TEXCOORD6;
    UNITY_VERTEX_OUTPUT_STEREO
};

// Three base PACKED_ROW_BASE digits, each a row shifted by one. Exact: the packed value stays below 2^24.
float3 UnpackStyleRows(float packedRows)
{
    float remaining = round(packedRows);
    float fillDigit = remaining - floor(remaining / PACKED_ROW_BASE) * PACKED_ROW_BASE;
    remaining = (remaining - fillDigit) / PACKED_ROW_BASE;
    float outlineDigit = remaining - floor(remaining / PACKED_ROW_BASE) * PACKED_ROW_BASE;
    float shadowDigit = (remaining - outlineDigit) / PACKED_ROW_BASE;
    return float3(fillDigit, outlineDigit, shadowDigit) - 1.0f;
}

Varyings Vertex(Attributes input)
{
    Varyings output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.worldPosition = input.positionOS;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uvAndPosition.xy = TRANSFORM_TEX(input.uv0.xy, _MainTex);
    output.uvAndPosition.zw = input.uv0.zw;
    output.edges = input.uv1;
    output.radii = input.uv2;
    output.effectParams = input.uv3.xyz;
    output.styleRows = UnpackStyleRows(input.uv3.w);
    // A mirrored element may flip the tangent's sign, which the aspect ratio must not follow.
    output.shadowOffsetAndAspect = float3(input.tangent.xy, abs(input.tangent.w));

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

// How far each edge distance shifts when the shape moves by the shadow offset. The offset arrives transformed
// like the vertices, so it's written in the screen steps of the canvas position, and since edge distances and
// canvas position are both affine across the shape, the shift takes the same steps of the edge distances.
float4 CalculateShadowShift(float2 canvasPosition, float4 edges, float2 shadowOffset)
{
    float2 positionStepX = ddx(canvasPosition);
    float2 positionStepY = ddy(canvasPosition);
    float4 edgeStepX = ddx(edges);
    float4 edgeStepY = ddy(edges);

    float determinant = positionStepX.x * positionStepY.y - positionStepX.y * positionStepY.x;
    // Seen edge-on, the element has no area on screen, so it has no shadow either.
    if (abs(determinant) < MIN_CANVAS_DETERMINANT)
        return 0.0f;

    float stepsX = (shadowOffset.x * positionStepY.y - shadowOffset.y * positionStepY.x) / determinant;
    float stepsY = (positionStepX.x * shadowOffset.y - positionStepX.y * shadowOffset.x) / determinant;
    return stepsX * edgeStepX + stepsY * edgeStepY;
}

half4 BlendOver(half4 top, half4 bottom)
{
    half alpha = top.a + bottom.a * (1.0h - top.a);
    half3 rgb = (top.rgb * top.a + bottom.rgb * bottom.a * (1.0h - top.a)) / max(alpha, MIN_ALPHA);
    return half4(rgb, alpha);
}

float4 Fragment(Varyings input) : SV_Target
{
    float2 position = input.uvAndPosition.zw;
    float aspectRatio = input.shadowOffsetAndAspect.z;
    int fillRow = (int)round(input.styleRows.x);
    int outlineRow = (int)round(input.styleRows.y);
    int shadowRow = (int)round(input.styleRows.z);
    float ringWidth = input.effectParams.x;
    float outlineWidth = input.effectParams.y;
    float outlineOutside = input.effectParams.z;

    bool hasOutline = outlineRow != NO_STYLE_ROW;
    float outset = hasOutline ? outlineWidth * outlineOutside : 0.0f;
    float inset = hasOutline ? outlineWidth - outset : 0.0f;

    // Coverages first: they use screen derivatives, which must not sit inside branches.
    float depth = -SdfRoundedQuad(input.edges, input.radii);
    // The outer edge (shape plus an outside outline) is antialiased once and the outline replaces the fill inside it,
    // so the fill never bleeds through the outline's antialiased border.
    float edgeCoverage = ShapeCoverage(depth + outset, ringWidth);
    float outlineMask = hasOutline ? Coverage(inset - depth) : 0.0f;
    float fillMask = outset > 0.0f ? Coverage(depth) : 1.0f;

    // The tint alpha carries canvas group fading, so it fades the shadow and the outline too.
    half tintAlpha = input.color.a;

    half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvAndPosition.xy);
    half4 fill = (sprite + _TextureSampleAdd) * input.color;
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

    // A real branch, so elements without a shadow skip its distance and coverage. The shadow row is the same for
    // every vertex of an element and a pixel quad never spans two triangles, so the branch is uniform per quad
    // and the derivatives inside it stay defined.
    UNITY_BRANCH
    if (shadowRow != NO_STYLE_ROW)
    {
        float2 shadowOffset = input.shadowOffsetAndAspect.xy;
        float4 shadowShift = CalculateShadowShift(input.worldPosition.xy, input.edges, shadowOffset);
        float shadowDepth = -SdfRoundedQuad(input.edges - shadowShift, input.radii);
        float shadowCoverage = ShapeCoverage(shadowDepth + outset, ringWidth);

        half4 shadow = (half4)LoadStyleTexel(shadowRow, STYLE_TEXEL_COLOR);
        shadow.a *= shadowCoverage * tintAlpha;
        result = BlendOver(result, shadow);
    }

    #ifdef UNITY_UI_CLIP_RECT
    result.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
    #endif

    clip(result.a - 0.001f);

    return result;
}

#endif
