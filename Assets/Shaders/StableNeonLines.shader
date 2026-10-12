Shader "NeonReflexVR/Stable Neon Lines"
{
    Properties
    {
        _BaseColor ("Line Color", Color) = (0.7, 0.12, 0.95, 1)
        _LineWidth ("Core Width (m)", Float) = 0.04
        _FadeStart ("Fade Start (m)", Float) = 8
        _FadeEnd ("Fade End (m)", Float) = 18
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Name "FilteredLine"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _LineWidth;
                float _FadeStart;
                float _FadeEnd;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 lineData : TEXCOORD0; // Along/cross distances in metres, then segment half length.
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 lineData : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.lineData = input.lineData;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 footprint = max(fwidth(input.lineData.xy), float2(0.00001, 0.00001));
                float2 distanceToCenter = abs(input.lineData.xy);
                float2 halfSize = float2(input.lineData.z, max(_LineWidth, 0.001) * 0.5);
                // Integrate the finite segment across the pixel footprint, including its end caps.
                float2 coverage = saturate((min(distanceToCenter + footprint * 0.5, halfSize)
                    - max(distanceToCenter - footprint * 0.5, -halfSize)) / footprint);
                float fade = 1 - smoothstep(_FadeStart, max(_FadeEnd, _FadeStart + 0.1),
                    distance(input.positionWS, GetCameraPositionWS()));
                half alpha = coverage.x * coverage.y * fade * _BaseColor.a;
                return half4(_BaseColor.rgb * alpha, alpha); // Premultiplied, no opaque support border.
            }
            ENDHLSL
        }
    }
    Fallback Off
}