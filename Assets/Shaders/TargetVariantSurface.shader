Shader "NeonReflexVR/Target Variant Surface"
{
    Properties
    {
        [HDR] _BaseColor ("Surface / Hit Color", Color) = (2.1, 0.45, 0.035, 1)
        _Energy ("Energy Surface", Range(0,1)) = 1
    }
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
                half _Energy;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half3 n=normalize(input.normalWS);
                half ndv=saturate(dot(n,GetWorldSpaceNormalizeViewDir(input.positionWS)));
                half diffuse=saturate(dot(n,normalize(half3(-0.4,0.7,-0.6))));
                half rim=pow(1-ndv,2);
                half armorLight=0.55h + diffuse*0.6h + rim*0.4h;
                half energyLight=0.82h + diffuse*0.15h + rim*0.12h;
                half3 color=_BaseColor.rgb*lerp(armorLight,energyLight,_Energy);
                color+=min(_BaseColor.rgb,half3(1,1,1))*pow(ndv,16)*(1-_Energy)*0.22h;
                // Existing renderer-wide _BaseColor MPB turns every submesh pure white.
                half whiteHit=1-step(0.01h,max(abs(_BaseColor.r-_BaseColor.g),abs(_BaseColor.g-_BaseColor.b)));
                return half4(lerp(color,_BaseColor.rgb,whiteHit),1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
