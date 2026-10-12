Shader "NeonReflexVR/Stable Neon Grid"
{
    Properties
    {
        _BaseColor ("Grid Color", Color) = (0.04, 0.7, 0.95, 1)
        _FloorColor ("Floor Color", Color) = (0.012, 0.018, 0.036, 1)
        _Spacing ("Spacing (m)", Float) = 1.5
        _LineWidth ("Line Width (m)", Float) = 0.03
        _FadeStart ("Fade Start (m)", Float) = 3
        _FadeEnd ("Fade End (m)", Float) = 7
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "GridUnlit"
            Tags { "LightMode"="SRPDefaultUnlit" }
            ZWrite On
            Cull Back
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _FloorColor;
                float _Spacing;
                float _LineWidth;
                float _FadeStart;
                float _FadeEnd;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float spacing = max(_Spacing, 0.1);
                float2 cell = input.positionWS.xz / spacing;
                float2 footprint = max(fwidth(cell), float2(0.00001, 0.00001));
                float2 distanceToLine = abs(frac(cell + 0.5) - 0.5);
                float halfWidth = clamp(_LineWidth / spacing, 0.001, 0.25) * 0.5;
                // Pixel box coverage instead of a binary edge: thin distant lines retain fractional energy.
                float2 coverage = saturate((min(distanceToLine + footprint * 0.5, halfWidth)
                    - max(distanceToLine - footprint * 0.5, -halfWidth)) / footprint);
                // When an entire cell is subpixel, converge to its average rather than flicker.
                coverage = lerp(coverage, halfWidth * 2, saturate(footprint - 1));
                float gridCoverage = 1 - (1 - coverage.x) * (1 - coverage.y);
                float distanceToEye = distance(input.positionWS, GetCameraPositionWS());
                float fade = 1 - smoothstep(_FadeStart, max(_FadeEnd, _FadeStart + 0.1), distanceToEye);
                return half4(lerp(_FloorColor.rgb, _BaseColor.rgb, gridCoverage * fade), 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}