Shader "URP/URPGlass"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" { }
        _NormalTex ("NormalTex", 2D) = "bump" { }
        _NormalScale ("NormalScale", Range(0, 1.5)) = 1.0
        _Offset ("Offset", Range(0, 10)) = 3
        _OffsetScale ("Offset Scale", Range(0, 5)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeLine" = "UniversalRenderPipeline" }

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _NormalScale;
            float _Offset;
            float _OffsetScale;
        CBUFFER_END

        float4 _CameraColorTexture_TexelSize;
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NormalTex);
        SAMPLER(sampler_NormalTex);
        TEXTURE2D(_CameraColorTexture);
        SAMPLER(sampler_CameraColorTexture);

        struct a2v
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
        };
        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float4 uv : TEXCOORD0;
            float4 normalWS : TEXCOORD1;
            float4 tangentWS : TEXCOORD2;
            float4 bitangentWS : TEXCOORD3;
        };

        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);

                o.uv.zw = o.positionCS.xy / o.positionCS.w * 0.5 + 0.5;
                #if UNITY_UV_STARTS_AT_TOP
                    o.uv.w = 1 - o.uv.w;
                #endif
                //o.uv.w=1-o.uv.w;
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS = float4(TransformObjectToWorldNormal(v.normalOS).xyz, positionWS.z);
                o.tangentWS = float4(TransformObjectToWorldDir(v.tangentOS).xyz, positionWS.x);
                o.bitangentWS = float4(cross(o.tangentWS.xyz, o.normalWS.xyz).xyz, positionWS.y);

                return o;
            }

            real4 frag(v2f i) : SV_TARGET
            {
                Light light = GetMainLight();

                float3x3 TBN = {
                    normalize(i.tangentWS.xyz), normalize(i.bitangentWS.xyz), normalize(i.normalWS.xyz)
                };

                float3 lightDirTS = normalize(mul(TBN, light.direction));
                float3 positionWS = float3(i.tangentWS.w, i.bitangentWS.w, i.normalWS.w);
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv), _NormalScale);
                normalTS.z = pow(1 - pow(normalTS.x, 2) - pow(normalTS.y, 2), 0.5);
                float nDotL = max(0, dot(lightDirTS, normalTS));

                float2 offset = _CameraColorTexture_TexelSize.xy * (normalTS.xy + _Offset) * _OffsetScale;

                // real4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy) * nDotL;
                real4 screenColor = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, i.uv.zw + offset);

                return screenColor;
            }
            ENDHLSL
        }
    }
}