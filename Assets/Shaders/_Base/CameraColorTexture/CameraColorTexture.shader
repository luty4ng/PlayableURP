Shader "Practical-URP/Base/CameraColorTexture"
{
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "AfterTransparents" }
            
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                return o;
            }

            float4 frag(v2f input) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_CameraColorTextureAlpha, sampler_CameraColorTextureAlpha, input.uv);
            }
            ENDHLSL
        }
    }
    FallBack off
}