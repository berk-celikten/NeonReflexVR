Shader "NeonReflexVR/Arcade Target"
{
    Properties { [HDR] _BaseColor ("Orb / Hit Color", Color) = (2.2, 0.24, 0.025, 1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
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
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; float latitude : TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.latitude = input.normalOS.y;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half3 n = normalize(input.normalWS);
                half ndv = saturate(dot(n, GetWorldSpaceNormalizeViewDir(input.positionWS)));
                half rim = pow(1 - ndv, 2);
                float footprint = max(fwidth(input.latitude), 0.001);
                half band = 1 - smoothstep(0.07 - footprint, 0.07 + footprint, abs(input.latitude));
                half light = 0.28h + 0.35h * saturate(dot(n, normalize(half3(-0.4, 0.7, -0.6))));
                // _BaseColor is the existing feedback property. White flash remains white, including rim/band.
                half whiteHit = 1 - step(0.01h, max(abs(_BaseColor.r - _BaseColor.g), abs(_BaseColor.g - _BaseColor.b)));
                half3 color = _BaseColor.rgb * (light + rim * 0.7h + band * 0.45h);
                color += min(_BaseColor.rgb, half3(1, 1, 1)) * pow(ndv, 12) * 0.18h;
                return half4(lerp(color, _BaseColor.rgb, whiteHit), 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}