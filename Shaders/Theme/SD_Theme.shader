Shader "UI/Theme"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Halftone properties
        _HalftoneTex ("Halftone Texture", 2D) = "white" {}
        _PatternOffset ("Pattern Offset", Vector) = (0,0,0,0)
        _PatternScale ("Pattern Scale", Vector) = (1,1,0,0)
        _PatternOpacity ("Pattern Opacity", Range(0,1)) = 0.2
        _BlendMode ("Blend Mode", Float) = 0
        _PatternRotation ("Pattern Rotation", Range(0, 360)) = 0
        [HideInInspector] _AspectRatio ("Aspect Ratio", Float) = 1

        // Mask component required properties
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
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
            Name "Default"

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma target 3.0

            #pragma multi_compile_local _ THEME_GRADIENT
            #pragma multi_compile_local _ HALFTONE_ON
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "ThemePass.hlsl"
            ENDHLSL
        }
    }
}