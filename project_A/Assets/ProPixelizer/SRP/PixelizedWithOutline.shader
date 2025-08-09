// Copyright Elliot Bentine, 2018-
Shader "ProPixelizer/SRP/PixelizedWithOutline"
{
    Properties
    {
        _LightingRamp("LightingRamp", 2D) = "white" {}
        _PaletteLUT("PaletteLUT", 2D) = "white" {}
        [MainTex][NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}
        _Albedo_ST("Albedo_ST", Vector) = (1, 1, 0, 0)
        [MainColor]_BaseColor("Color", Color) = (1, 1, 1, 1)
        _AmbientLight("AmbientLight", Color) = (0.2, 0.2, 0.2, 1.0)
        [IntRange] _PixelSize("PixelSize", Range(1, 5)) = 3
        _PixelGridOrigin("PixelGridOrigin", Vector) = (0, 0, 0, 0)
        [Normal][NoScaleOffset]_NormalMap("Normal Map", 2D) = "bump" {}
        _NormalMap_ST("Normal Map_ST", Vector) = (1, 1, 0, 0)
        [NoScaleOffset]_Emission("Emission", 2D) = "white" {}
        _Emission_ST("Emission_ST", Vector) = (1, 1, 0, 0)
        _EmissionColor("EmissionColor", Color) = (1, 1, 1, 0)
        _AlphaClipThreshold("Alpha Clip Threshold", Range(0, 1)) = 0.5
        [IntRange] _ID("ID", Range(0, 255)) = 1
        _OutlineColor("OutlineColor", Color) = (0.0, 0.0, 0.0, 0.5)
        _EdgeHighlightColor("Edge Highlight Color", Color) = (0.5, 0.5, 0.5, 0)
        _DiffuseVertexColorWeight("DiffuseVertexColorWeight", Range(0, 1)) = 1
        _EmissiveVertexColorWeight("EmissiveVertexColorWeight", Range(0, 1)) = 0
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
        [Toggle]COLOR_GRADING("Use Color Grading", Float) = 0
        [Toggle]USE_OBJECT_POSITION("Use Object Position", Float) = 1
        [Toggle]RECEIVE_SHADOWS("ReceiveShadows", Float) = 1
        [Toggle]PROPIXELIZER_DITHERING("Use Dithering", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        UsePass "ProPixelizer/Hidden/ProPixelizerBase/UNIVERSAL FORWARD"
        UsePass "ProPixelizer/Hidden/ProPixelizerBase/SHADOWCASTER"
        UsePass "ProPixelizer/Hidden/ProPixelizerBase/DEPTHONLY"
        UsePass "ProPixelizer/Hidden/ProPixelizerBase/DEPTHNORMALS"

        Pass
        {
            Name "ProPixelizerPass"
            Tags {
                "RenderPipeline"="UniversalPipeline"
                "LightMode"="ProPixelizer"
                "DisableBatching"="True"
            }

            ZWrite On
            Cull Off
            Blend Off

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "PixelUtils.hlsl"
            #include "PackingUtils.hlsl"
            #include "ScreenUtils.hlsl" 

            // === INSTANCING ENABLE ===
            #pragma multi_compile_instancing
            // (URP 렌더링 레이어를 인스턴싱과 함께 쓰려면 아래 옵션 권장)
            #pragma instancing_options renderinglayer

            #pragma vertex outline_vert
            #pragma fragment outline_frag
            #pragma target 2.5
            #pragma multi_compile_local USE_OBJECT_POSITION_ON _
            #pragma multi_compile USE_ALPHA_ON _
            #pragma multi_compile NORMAL_EDGE_DETECTION_ON _
            #pragma multi_compile_local PROPIXELIZER_DITHERING_ON _

            // Graph Properties (SRP Batcher용 CBUFFER - 원본 유지)
            CBUFFER_START(UnityPerMaterial)
                float4 _LightingRamp_TexelSize;
                float4 _PaletteLUT_TexelSize;
                float4 _Albedo_TexelSize;
                float4 _Albedo_ST;
                float4 _BaseColor;
                float4 _AmbientLight;
                float _PixelSize;
                float4 _PixelGridOrigin;
                float4 _NormalMap_TexelSize;
                float4 _NormalMap_ST;
                float4 _Emission_TexelSize;
                float4 _Emission_ST;
                float _AlphaClipThreshold;
                float _ID;
                float4 _OutlineColor;
                float4 _EdgeHighlightColor;
                float4 _EmissionColor;
                float _DiffuseVertexColorWeight;
                float _EmissiveVertexColorWeight;
            CBUFFER_END

            // Textures/Samplers
            SAMPLER(SamplerState_Linear_Repeat);
            SAMPLER(SamplerState_Point_Clamp);
            SAMPLER(SamplerState_Point_Repeat);
            TEXTURE2D(_LightingRamp); SAMPLER(sampler_LightingRamp);
            TEXTURE2D(_PaletteLUT);   SAMPLER(sampler_PaletteLUT);
            TEXTURE2D(_Albedo);       SAMPLER(sampler_Albedo);
            TEXTURE2D(_NormalMap);    SAMPLER(sampler_NormalMap);
            TEXTURE2D(_Emission);     SAMPLER(sampler_Emission);

            // === INSTANCING PROPS (Per-Instance overrides) ===
            // 인스턴스마다 색/ID를 바꾸고 싶을 때만 셋업하면 되고,
            // 셋업 안 하면 CBUFFER(_BaseColor/_ID) 값이 그대로 사용됩니다.
            #ifdef UNITY_INSTANCING_ENABLED
            UNITY_INSTANCING_BUFFER_START(PerInstance)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor_Instance)
                UNITY_DEFINE_INSTANCED_PROP(float,  _ID_Instance)
            UNITY_INSTANCING_BUFFER_END(PerInstance)

            // 기존 코드/Include에서 쓰는 심볼을 인스턴싱 값으로 alias
            // (인스턴싱 값이 없다면 CBUFFER 값이 들어가도록 폴백)
            #define _BaseColor (UNITY_ACCESS_INSTANCED_PROP(PerInstance, _BaseColor_Instance))
            #define _ID        (UNITY_ACCESS_INSTANCED_PROP(PerInstance, _ID_Instance))
            #endif

            #include "OutlinePass.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "ProPixelizer.PixelizedWithOutlineShaderGUI"
    FallBack "ProPixelizer/Hidden/ProPixelizerBase"
}
