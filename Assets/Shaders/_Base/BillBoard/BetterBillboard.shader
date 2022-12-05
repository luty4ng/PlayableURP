Shader "Practical-URP/Base/BetterBillBoard"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "White" { }
        [HDR]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        _Rotate ("Rotate", Range(0, 3.14)) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" "Queue" = "Overlay" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _BaseColor;
            float _Rotate;
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
            float4 color : COLOR;
        };
        ENDHLSL

        pass
        {

            Tags { "LightMode" = "UniversalForward" "RenderType" = "Overlay" }

            Blend one one

            ZWrite off

            ZTest always

            HLSLPROGRAM
            #pragma vertex VERT
            #pragma fragment FRAG

            v2f VERT(a2v i)
            {
                v2f o;
                o.texcoord = TRANSFORM_TEX(i.texcoord, _MainTex);

                float4 pivotWS = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1));
                float4 pivotVS = mul(UNITY_MATRIX_V, pivotWS);

                float ScaleX = length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x));
                float ScaleY = length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y));

                //定义一个旋转矩阵
                float2x2 rotateMatrix = {
                    cos(_Rotate), -sin(_Rotate), sin(_Rotate), cos(_Rotate)
                };

                //用来临时存放旋转后的坐标
                float2 pos = i.positionOS.xy * float2(ScaleX, ScaleY);
                pos = mul(rotateMatrix, pos);
                float4 positionVS = pivotVS + float4(pos, 0, 1); //深度取的轴心位置深度，xy进行缩放

                o.positionCS = mul(UNITY_MATRIX_P, positionVS);
                o.color = _BaseColor * _BaseColor.a;
                return o;
            }

            half4 FRAG(v2f i) : SV_TARGET
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                return tex * i.color;
            }
            ENDHLSL
        }
    }
}