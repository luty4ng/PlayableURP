Shader "Practical-URP/PostProcessing/KawaseBlur"
{

    Properties
    {
        _MainTex ("MainTex", 2D) = "white" { }
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }
        Cull Off ZWrite Off ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            float _Blur;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
        CBUFFER_END
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct a2v
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD;
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD;
        };
        ENDHLSL

        pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                return o;
            }

            half4 frag(v2f i) : SV_TARGET
            {
                float4 col;
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                tex += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-1, -1) * _MainTex_TexelSize.xy * _Blur);
                tex += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(1, -1) * _MainTex_TexelSize.xy * _Blur);
                tex += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-1, 1) * _MainTex_TexelSize.xy * _Blur);
                tex += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(1, 1) * _MainTex_TexelSize.xy * _Blur);
                return tex / 5.0;
            }
            ENDHLSL
        }
    }
}