Shader "NeonReflexVR/Arcade Beam"
{
    Properties { [HDR] _BaseColor ("Beam Color", Color) = (0.12, 1.25, 1.65, 1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
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
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float distanceToCore = abs(input.uv.y - 0.5);
                float footprint = max(fwidth(input.uv.y), 0.001);
                half core = saturate((min(distanceToCore + footprint * 0.5, 0.11)
                    - max(distanceToCore - footprint * 0.5, -0.11)) / footprint);
                half glow = pow(saturate(1 - distanceToCore * 2), 3) * 0.32h;
                half cap = smoothstep(0, 0.012, input.uv.x) * (1 - smoothstep(0.988, 1, input.uv.x));
                half alpha = saturate(core + glow) * cap * input.color.a;
                half3 color = lerp(_BaseColor.rgb, min(_BaseColor.rgb, half3(1, 1, 1)) + 0.55h, core * 0.5h);
                return half4(color * input.color.rgb * alpha, alpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}