Shader "Practical-URP/PostProcessing/SimplePostProcessing"
{

    Properties
    {

        [HideInInspector]_MainTex ("MainTex", 2D) = "white" { }
        _Brightness ("Brightness", Range(0, 1)) = 1
        _Saturate ("Saturate", Range(0, 1)) = 1
        _Contranst ("Constrast", Range(-1, 2)) = 1
    }

    SubShader
    {

        Tags { "RenderPipeline" = "UniversalRenderPipeline" }
        Cull Off ZWrite Off ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float _Brightness;
            float _Saturate;
            float _Contranst;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct a2v
        {
            float4 positionOS : POSITION;
            float2 texcoord : TEXCOORD;
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float2 texcoord : TEXCOORD;
        };
        ENDHLSL

        pass
        {

            HLSLPROGRAM
            #pragma vertex VERT
            #pragma fragment FRAG
            v2f VERT(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.texcoord = i.texcoord;
                return o;
            }

            half4 FRAG(v2f i) : SV_TARGET
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                float gray = 0.21 * tex.x + 0.72 * tex.y + 0.072 * tex.z; //明度
                tex.xyz *= _Brightness;//亮度
                tex.xyz = lerp(float3(gray, gray, gray), tex.xyz, _Saturate);//饱和度
                tex.xyz = lerp(float3(0.5, 0.5, 0.5), tex.xyz, _Contranst);//对比度
                return tex;
            }
            ENDHLSL
        }
    }
}