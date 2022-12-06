Shader "Practical-URP/PostProcessing/RadialBlur"
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
        float4 _MainTex_ST;
        float _Iteration;
        float _Blur;
        float _Y;
        float _X;
        float _Instensity;
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_SourceTex);
        SAMPLER(sampler_SourceTex);

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
                float4 color = 0;
                float2 dir = (i.uv - float2(_X, _Y)) * _Blur * 0.01;
                for (int iter = 0; iter < _Iteration; iter++)
                {
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + dir * iter) / _Iteration;
                }
                return color;
            }
            ENDHLSL
        }
        
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
                float4 blur = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 source = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, i.uv);
                return lerp(source, blur, _Instensity);
            }
            ENDHLSL
        }
    }
}