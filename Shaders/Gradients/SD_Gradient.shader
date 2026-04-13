Shader "UI/Gradient"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        _Direction ("Direction", Int) = 0
        _Rotation ("Rotation", Range(0, 360)) = 0
        _Color ("Tint", Color) = (1,1,1,1)

        // Halftone properties
        _HalftoneTex ("Halftone Texture", 2D) = "white" {}
        _PatternOffset ("Pattern Offset", Vector) = (0,0,0,0)
        _PatternScale ("Pattern Scale", Vector) = (1,1,0,0)
        _PatternOpacity ("Pattern Opacity", Range(0,1)) = 0.2
        _DotColor ("Dot Color", Color) = (0,0,0,1)
        _PatternRotation ("Pattern Rotation", Range(0, 360)) = 0

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
            #pragma target 2.0

            #pragma multi_compile_local _ GRADIENT_LINEAR GRADIENT_DIAMOND GRADIENT_RADIAL GRADIENT_ANGULAR
            #pragma multi_compile_local _ HALFTONE_ON
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "GradientPass.hlsl"
            ENDHLSL
        }
    }
}