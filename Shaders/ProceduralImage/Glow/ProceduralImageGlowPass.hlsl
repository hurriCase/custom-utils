#ifndef PROCEDURAL_IMAGE_GLOW_INCLUDED
#define PROCEDURAL_IMAGE_GLOW_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.firsttry.customutils/Shaders/Shared/Common.hlsl"

#define SQRT_2 1.41421356f
#define MIN_GLOW_SIZE 0.001f
#define POW_EPSILON 0.000001f

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
    float4 uv3 : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float4 color : COLOR;
    float4 worldPosition : TEXCOORD0;
    float3 glowParams : TEXCOORD1;
    float4 cornersTop : TEXCOORD2;
    float4 cornersBottom : TEXCOORD3;
    float4 radii : TEXCOORD4;
    UNITY_VERTEX_OUTPUT_STEREO
};

float Erf7(float x)
{
    x *= 1.12837917f;
    float xx = x * x;
    x = x + (0.24295f + (0.03395f + 0.0104f * xx) * xx) * (x * xx);
    return x / sqrt(1.0f + x * x);
}

float DistanceToEdge(float2 a, float2 b, float orientation)
{
    float2 edge = b - a;
    float2 direction = edge / max(length(edge), MIN_GLOW_SIZE);
    float2 outward = float2(-direction.y, direction.x) * orientation;
    return -dot(a, outward);
}

struct QuadEdges
{
    float top;
    float right;
    float bottom;
    float left;
};

QuadEdges CalculateQuadEdges(float2 topLeft, float2 topRight, float2 bottomRight, float2 bottomLeft)
{
    float doubleArea = Cross2D(topLeft, topRight) + Cross2D(topRight, bottomRight)
        + Cross2D(bottomRight, bottomLeft) + Cross2D(bottomLeft, topLeft);
    float orientation = doubleArea < 0.0f ? 1.0f : -1.0f;

    QuadEdges edges;
    edges.top = DistanceToEdge(topLeft, topRight, orientation);
    edges.right = DistanceToEdge(topRight, bottomRight, orientation);
    edges.bottom = DistanceToEdge(bottomRight, bottomLeft, orientation);
    edges.left = DistanceToEdge(bottomLeft, topLeft, orientation);
    return edges;
}

float BlurredRoundedQuad(QuadEdges edges, float width, float height, float4 radii, float sigma, float spread)
{
    edges.top -= spread;
    edges.right -= spread;
    edges.bottom -= spread;
    edges.left -= spread;

    width = max(width + 2.0f * spread, MIN_GLOW_SIZE);
    height = max(height + 2.0f * spread, MIN_GLOW_SIZE);

    float horizontalEdge = max(edges.left, edges.right);
    float verticalEdge = max(edges.top, edges.bottom);

    float band = max(sigma, 1.0f);
    float rightWeight = smoothstep(-band, band, 0.5f * (edges.right - edges.left));
    float topWeight = smoothstep(-band, band, 0.5f * (edges.top - edges.bottom));
    float radius = lerp(lerp(radii.w, radii.z, rightWeight), lerp(radii.x, radii.y, rightWeight), topWeight);
    radius = radius > POW_EPSILON ? max(radius + spread, 0.0f) : 0.0f;

    float minEdge = min(width, height);
    float maxRadius = 0.5f * minEdge;
    radius = min(radius, maxRadius);

    float s = max(sigma, MIN_GLOW_SIZE) * SQRT_2;
    float sInv = 1.0f / s;

    float r0 = min(sqrt(radius * radius + 1.3225f * s * s), maxRadius);
    float r1 = min(sqrt(radius * radius + 4.0f * s * s), maxRadius);
    float exponent = 2.0f * r1 / max(r0, MIN_GLOW_SIZE);

    float halfWidthRatio = 0.5f * sInv * width;
    float halfHeightRatio = 0.5f * sInv * height;
    float delta = 1.25f * s * (exp(-halfWidthRatio * halfWidthRatio) - exp(-halfHeightRatio * halfHeightRatio));
    float adjustedWidth = width + min(delta, 0.0f);
    float adjustedHeight = height - max(delta, 0.0f);

    float x0 = horizontalEdge - 0.5f * min(delta, 0.0f) + r1;
    float y0 = verticalEdge + 0.5f * max(delta, 0.0f) + r1;

    float positiveDistance = pow(
        pow(max(x0, 0.0f) + POW_EPSILON, exponent) + pow(max(y0, 0.0f) + POW_EPSILON, exponent),
        1.0f / exponent);
    float negativeDistance = min(max(x0, y0), 0.0f);
    float distance = positiveDistance + negativeDistance - r1;

    float scale = 0.5f * Erf7(sInv * 0.5f * (max(adjustedWidth, adjustedHeight) - 0.5f * radius));
    return scale * (Erf7(sInv * (minEdge + distance)) - Erf7(sInv * distance));
}

float SourceCoverage(QuadEdges edges, float4 radii, float falloff)
{
    bool isRight = edges.right > edges.left;
    bool isTop = edges.top > edges.bottom;
    float radius = isRight ? isTop ? radii.y : radii.z : isTop ? radii.x : radii.w;

    float2 q = float2(max(edges.left, edges.right), max(edges.top, edges.bottom)) + radius;
    float sdf = length(max(q, 0.0f)) + min(max(q.x, q.y), 0.0f) - radius;

    float pixelScale = clamp(1.0f / max(falloff, MIN_PIXEL_WORLD_SCALE), MIN_PIXEL_WORLD_SCALE, MAX_PIXEL_WORLD_SCALE);
    return saturate(-sdf * pixelScale);
}

Varyings Vertex(Attributes input)
{
    Varyings output;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.worldPosition = input.positionOS;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    output.glowParams = input.uv0.xyz;
    output.cornersTop = input.uv1;
    output.cornersBottom = input.uv2;
    output.radii = input.uv3;

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

    float2 topLeft = input.cornersTop.xy;
    float2 topRight = input.cornersTop.zw;
    float2 bottomRight = input.cornersBottom.xy;
    float2 bottomLeft = input.cornersBottom.zw;

    float sigma = input.glowParams.x;
    float spread = input.glowParams.y;
    float maskFalloff = input.glowParams.z;

    float width = 0.5f * (length(topRight - topLeft) + length(bottomRight - bottomLeft));
    float height = 0.5f * (length(topLeft - bottomLeft) + length(topRight - bottomRight));

    QuadEdges edges = CalculateQuadEdges(topLeft, topRight, bottomRight, bottomLeft);

    color.a *= BlurredRoundedQuad(edges, width, height, input.radii, sigma, spread);

    if (maskFalloff >= 0.0f)
        color.a *= 1.0f - SourceCoverage(edges, input.radii, maskFalloff);

    clip(color.a - 0.00001f);

    return color;
}

#endif
