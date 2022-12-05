Shader "Practical-URP/Base/WorldSpacePosition"
{
    Properties
    {
        [HideInInspector] _MainTex ("MainTex", 2D) = "white" { }
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }

        Cull Off ZWrite Off ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4x4 _CornerMatrix;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);

        struct a2v
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float2 uv_depth : TEXCOORD1;
            float3 interpolatedRay : TEXCOORD2;
        };

        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(a2v input)
            {
                v2f output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                output.uv_depth = input.uv;

                int index = 0;
                if (input.uv.x < 0.5 && input.uv.y < 0.5)
                    index = 0;
                else if (input.uv.x > 0.5 && input.uv.y < 0.5)
                    index = 1;
                else if (input.uv.x > 0.5 && input.uv.y > 0.5)
                    index = 2;
                else
                    index = 3;
                
                output.interpolatedRay = _CornerMatrix[index].xyz;
                return output;
            }

            real4 frag(v2f input) : SV_TARGET
            {
                real4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv_depth);
                half linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float3 fragPosVS = linearDepth * input.interpolatedRay;
                float3 fragPosWS = _WorldSpaceCameraPos + fragPosVS + float3(0.1, 0.1, 0.1);
                return mainTex + step(0, fragPosWS.x) * real4(1, 0, 0, 0) + step(0, fragPosWS.y) * real4(0, 1, 0, 0) + step(0, fragPosWS.z) * real4(0, 0, 1, 0);
            }

            ENDHLSL
        }
    }
}