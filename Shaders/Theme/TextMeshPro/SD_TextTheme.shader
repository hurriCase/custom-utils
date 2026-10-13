Shader "TextMeshPro/Distance Field Theme"
{
    Properties
    {
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0

        // Outline properties
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Thickness", Range(0,1)) = 0
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0

        // Underlay properties
        _UnderlayColor ("Underlay Color", Color) = (0,0,0,0.5)
        _UnderlayOffsetX ("Underlay Offset X", Range(-1,1)) = 0
        _UnderlayOffsetY ("Underlay Offset Y", Range(-1,1)) = 0
        _UnderlayDilate ("Underlay Dilate", Range(-1,1)) = 0
        _UnderlaySoftness ("Underlay Softness", Range(0,1)) = 0

        // Font atlas properties, set by TextMesh Pro
        _MainTex ("Font Atlas", 2D) = "white" {}
        _TextureWidth ("Texture Width", Float) = 512
        _TextureHeight ("Texture Height", Float) = 512
        _GradientScale ("Gradient Scale", Float) = 5
        _ScaleX ("Scale X", Float) = 1
        _ScaleY ("Scale Y", Float) = 1
        _PerspectiveFilter ("Perspective Correction", Range(0,1)) = 0.875
        _Sharpness ("Sharpness", Range(-1,1)) = 0
        _WeightNormal ("Weight Normal", Float) = 0
        _WeightBold ("Weight Bold", Float) = 0.5
        _ScaleRatioA ("Scale Ratio A", Float) = 1
        _ScaleRatioB ("Scale Ratio B", Float) = 1
        _ScaleRatioC ("Scale Ratio C", Float) = 1
        _VertexOffsetX ("Vertex Offset X", Float) = 0
        _VertexOffsetY ("Vertex Offset Y", Float) = 0
        _ClipRect ("Clip Rect", Vector) = (-32767,-32767,32767,32767)
        _MaskSoftnessX ("Mask Softness X", Float) = 0
        _MaskSoftnessY ("Mask Softness Y", Float) = 0

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
        [HideInInspector] _CullMode ("Cull Mode", Float) = 0
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull [_CullMode]
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma target 3.0

            #pragma multi_compile_local _ UNDERLAY_ON
            #pragma multi_compile_local _ THEME_GRADIENT
            #pragma multi_compile_local _ HALFTONE_ON
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "TextThemePass.hlsl"
            ENDHLSL
        }
    }
}
