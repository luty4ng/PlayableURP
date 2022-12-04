Shader "Extended-URP/URP/序列帧"
{

    Properties
    {
        _MainTex ("MainTex", 2D) = "white" { }
        _BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        _Sheet ("Sheet", Vector) = (1, 1, 1, 1)
        _FrameRate ("FrameRate", float) = 25
    }

    SubShader
    {

        Tags { "RenderPipeline" = "UniversalRenderPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _BaseColor;
            half4 _Sheet;
            float _FrameRate;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct a2v
        {
            float4 positionOS : POSITION;
            float4 normalOS : NORMAL;
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

            Tags { "LightMode" = "UniversalForward" }
            ZWrite off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex VERT
            #pragma fragment FRAG

            v2f VERT(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.texcoord = TRANSFORM_TEX(i.texcoord, _MainTex);
                return o;
            }

            half4 FRAG(v2f i) : SV_TARGET
            {
                float2 uv;
                uv.x = i.texcoord.x / _Sheet.x + frac(floor(_Time.y * _FrameRate) / _Sheet.x);
                uv.y = i.texcoord.y / _Sheet.y + 1 - frac(floor(_Time.y * _FrameRate / _Sheet.x) / _Sheet.y);
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            }
            ENDHLSL
        }
    }
}