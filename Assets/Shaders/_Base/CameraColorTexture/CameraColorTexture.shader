Shader "/Extended-URP/Base/CameraColorTexture"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
        
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
                // //根据时间改变采样噪声图获得随机的输出
                // float4 noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex,
                //                                 input.uv - _Time.xy * _DistortTimeFactor);
                // //以随机的输出*控制系数得到偏移值
                // float2 offset = noise.xy * _DistortStrength;
                // //采样Mask图获得权重信息
                // float4 factor = SAMPLE_TEXTURE2D(_MaskSoildColor, sampler_MaskSoildColor, input.uv);
                //像素采样时偏移offset，用Mask权重进行修改
                float2 uv = input.uv;
                return SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);
            }
            ENDHLSL
        }
    }
    FallBack off
}