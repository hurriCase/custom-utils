Shader "Custom/UI/MultiGradient"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        _Direction ("Direction", Int) = 1
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _GradientTex;
            int _Direction;
            float4 _Color;

            // Direction enum values:
            // None = 0, LeftToRight = 1, TopToBottom = 2, RightToLeft = 3, BottomToTop = 4

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t;

                if (_Direction == 1)      t = i.uv.x;
                else if (_Direction == 2) t = 1.0 - i.uv.y;
                else if (_Direction == 3) t = 1.0 - i.uv.x;
                else if (_Direction == 4) t = i.uv.y;
                else                      t = i.uv.x;

                fixed4 gradientColor = tex2D(_GradientTex, float2(t, 0));
                fixed4 mainTex = tex2D(_MainTex, i.uv);

                return gradientColor * mainTex * i.color * _Color;
            }
            ENDCG
        }
    }
}