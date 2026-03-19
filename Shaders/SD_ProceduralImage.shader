Shader "UI/Procedural UI Image"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 worldPosition : TEXCOORD0;
                float4 radius : TEXCOORD1;
                float2 texcoord : TEXCOORD2;
                float2 size : TEXCOORD3;
                float lineWeight : TEXCOORD4;
                float pixelWorldScale : TEXCOORD5;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            float4 _ClipRect;
            int _UIVertexColorAlwaysGammaSpace;

            static const float Max16BitValue = 65535.0f;
            static const float MaxPixelWorldScale = 2048.0f;
            static const float MinPixelWorldScale = 1.0f / MaxPixelWorldScale;

            float2 decode2(float value)
            {
                float2 encodeMul = float2(1.0f, Max16BitValue);
                float encodeBit = 1.0f / Max16BitValue;
                float2 enc = encodeMul * value;
                enc = frac(enc);
                enc.x -= enc.y * encodeBit;
                return enc;
            }

            v2f vert(appdata_t input)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = input.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.size = input.uv1;
                OUT.texcoord = input.uv0;

                float minside = min(OUT.size.x, OUT.size.y);

                OUT.lineWeight = input.uv3.x * minside;
                OUT.radius = float4(decode2(input.uv2.x), decode2(input.uv2.y)) * minside;
                OUT.pixelWorldScale = clamp(input.uv3.y, MinPixelWorldScale, MaxPixelWorldScale);

                if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace())
                    input.color.rgb = UIGammaToLinear(input.color.rgb);

                OUT.color = input.color * _Color;
                return OUT;
            }

            float visible(float2 position, float2 halfSize, float4 radii)
            {
                float cornerRadius = position.x > 0.0f
                    ? position.y > 0.0f ? radii.y : radii.z
                    : position.y > 0.0f ? radii.x : radii.w;

                float2 distanceToCornerCenter = abs(position) - halfSize + cornerRadius;
                float distanceToRoundedCorner = length(max(distanceToCornerCenter, 0.0f));
                float distanceToStraightEdge = min(max(distanceToCornerCenter.x, distanceToCornerCenter.y), 0.0f);
                return cornerRadius - distanceToRoundedCorner - distanceToStraightEdge;
            }

            fixed4 frag(v2f IN) : SV_Target
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
                float sdf = visible(centeredPosition, halfSize, IN.radius);
                float borderCenter = (IN.lineWeight + 1.0f / IN.pixelWorldScale) * 0.5f;
                color.a *= saturate((borderCenter - abs(sdf - borderCenter)) * IN.pixelWorldScale);

                if (color.a <= 0)
                    discard;

                return color;
            }
            ENDCG
        }
    }
}