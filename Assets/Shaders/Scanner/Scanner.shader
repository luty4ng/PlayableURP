Shader "Practical-URP/PostProcessing/Scanner"
{
    Properties
    {
        [HideInInspector] _MainTex ("MainTex", 2D) = "white" { }
        _DetailTex ("DetailTex", 2D) = "white" { }
        _ScanDistance ("Scan Distance", float) = 10
        _ScanWidth ("Scan Width", float) = 10
        _LeadSharp ("Leading Edge Sharpness", float) = 10
        [HDR]_LeadColor ("Leading Edge Color", Color) = (1, 1, 1, 0)
        [HDR]_MidColor ("Mid Color", Color) = (1, 1, 1, 0)
        [HDR]_TrailColor ("Trail Color", Color) = (1, 1, 1, 0)
        [HDR]_HBarColor ("Horizontal Bar Color", Color) = (0.5, 0.5, 0.5, 0)
        [HDR]_OutlineColor ("OutlineColr", Color) = (1, 1, 1, 1)
        [Range(0.0, 2.0)]_NormalSensitivity ("NormalSensitivity", float) = 1
        _DepthSensitivity ("DepthSensitivity", float) = 1
        _Speed ("Speed", float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }

        Cull Off ZWrite Off ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4x4 _CornerMatrix;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_DetailTex);
        SAMPLER(sampler_DetailTex);
        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);
        TEXTURE2D(_CameraDepthNormalsTexture);
        SAMPLER(sampler_CameraDepthNormalsTexture);

        float4 _WorldSpaceScannerPos;
        float _ScanDistance;
        float _ScanWidth;
        float _LeadSharp;
        float4 _LeadColor;
        float4 _MidColor;
        float4 _TrailColor;
        float4 _HBarColor;
        float4 _OutlineColor;
        float _NormalSensitivity;
        float _DepthSensitivity;


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
            real2 uv_sobel[9] : TEXCOORD3;
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
                output.uv_sobel[0] = input.uv + _MainTex_TexelSize.xy * real2(-1, -1);
                output.uv_sobel[1] = input.uv + _MainTex_TexelSize.xy * real2(0, -1);
                output.uv_sobel[2] = input.uv + _MainTex_TexelSize.xy * real2(1, -1);
                output.uv_sobel[3] = input.uv + _MainTex_TexelSize.xy * real2(-1, 0);
                output.uv_sobel[4] = input.uv + _MainTex_TexelSize.xy * real2(0, 0);
                output.uv_sobel[5] = input.uv + _MainTex_TexelSize.xy * real2(1, 0);
                output.uv_sobel[6] = input.uv + _MainTex_TexelSize.xy * real2(-1, 1);
                output.uv_sobel[7] = input.uv + _MainTex_TexelSize.xy * real2(0, 1);
                output.uv_sobel[8] = input.uv + _MainTex_TexelSize.xy * real2(1, 1);
                return output;
            }
            
            float4 horizBars(float2 uv)
            {
                return 1 - saturate(round(abs(frac(uv.y * 100) * 2)));
            }

            float4 horizTex(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_DetailTex, sampler_DetailTex, float2(uv.x * 30, uv.y * 40));
            }

            // inline float DecodeFloatRG(float2 enc)
            // {
            //     float2 kDecodeDot = float2(1.0, 1 / 255.0);
            //     return dot(enc, kDecodeDot);
            // }
            
            // inline float3 DecodeNormal(float4 enc)
            // {
            //     float kScale = 1.7777;
            //     float3 nn = enc.xyz * float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
            //     float g = 2.0 / dot(nn.xyz, nn.xyz);
            //     float3 n;
            //     n.xy = g * nn.xy;
            //     n.z = g - 1;
            //     return n;
            // }
            
            // inline  void  DecodeDepthNormal(float4 enc, out float depth, out float3 normal)
            // {
            //     depth = DecodeFloatRG(enc.zw);
            //     normal = DecodeNormal(enc);
            // }

            int sobel(v2f i);

            real4 frag(v2f input) : SV_TARGET
            {
                int outline = sobel(input);
                real4 outlineColor = outline * _OutlineColor;
                // return outlineColor;
                real4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                real4 depthNormalTex = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, input.uv);
                // return depthNormalTex;
                real linear01Depth = depthNormalTex.z * 1.0 + depthNormalTex.w / 255.0;
                real linearDepth = linear01Depth * _ProjectionParams.z;

                // real depth;
                // real3 normal;
                // DecodeDepthNormal(depthNormalTex, depth, normal);
                // return depth;

                real rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv_depth);
                // real rawLinearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                real rawLinear01Depth = Linear01Depth(rawDepth, _ZBufferParams);
                // return real4(rawLinear01Depth, rawLinear01Depth, rawLinear01Depth, 1) * 50;
                
                float3 fragPosVS = linearDepth * input.interpolatedRay;
                float3 fragPosWS = _WorldSpaceCameraPos + fragPosVS;
                
                float dist = distance(fragPosWS, _WorldSpaceScannerPos);
                real4 scannerColor = real4(0, 0, 0, 0);
                if (dist < _ScanDistance && dist > _ScanDistance - _ScanWidth && rawLinear01Depth < 1)
                {
                    float diff = 1 - (_ScanDistance - dist) / (_ScanWidth);
                    real4 edge = lerp(_MidColor, _LeadColor, pow(diff, _LeadSharp));
                    scannerColor = lerp(_TrailColor, edge, diff) + horizBars(input.uv) * _HBarColor;
                    scannerColor *= diff;
                }
                float alpha = smoothstep(_ScanDistance - 5, _ScanDistance, dist);
                float outlineMask = alpha * saturate((_ScanDistance - dist) * 100) * step(0, depthNormalTex.z - 0.001);
                // return outlineMask * outlineColor;
                return mainTex + scannerColor + outlineColor * outlineMask;
            }

            int sobel(v2f i)
            {
                const real Gx[9] = {
                    - 1, 0, 1,
                    - 2, 0, 2,
                    - 1, 0, 1
                };
                const real Gy[9] = {
                    - 1, -2, -1,
                    0, 0, 0,
                    1, 2, 1
                };

                real depthEdgeX, depthEdgeY = 0;
                real normalEdgeX, normalEdgeY = 0;
                for (int index = 0; index < 9; index++)
                {
                    real4 depthnormalTex = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.uv_sobel[index]);
                    real2 normalTex = depthnormalTex.xy;
                    real depthTex = depthnormalTex.z * 1.0 + depthnormalTex.w / 255.0;
                    depthEdgeX += depthTex * Gx[index];
                    depthEdgeY += depthTex * Gy[index];
                    normalEdgeX += normalTex * Gx[index];
                    normalEdgeY += normalTex * Gy[index];
                }
                
                real depthGradient = abs(depthEdgeX) + abs(depthEdgeY);
                real normalGradient = abs(normalEdgeX) + abs(normalEdgeY);
                return (normalGradient) * _DepthSensitivity + (1 - depthGradient) * _NormalSensitivity;
            }

            ENDHLSL
        }
    }
}