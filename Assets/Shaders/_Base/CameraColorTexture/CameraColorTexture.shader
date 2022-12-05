Shader "Practical-URP/Base/CameraColorTexture"
{
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "AfterTransparents" "RenderPipeline" = "UniversalPipeline"}
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraColorTextureAlpha);
            SAMPLER(sampler_CameraColorTextureAlpha);
        
            struct a2v
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(a2v input)
            {
                v2f output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;
                return output;
            }

            float4 frag(v2f input) : SV_Target
            {
                float2 uv = input.uv;
                return SAMPLE_TEXTURE2D(_CameraColorTextureAlpha, sampler_CameraColorTextureAlpha, uv);
            }
            ENDHLSL
        }
    }
    FallBack off
}