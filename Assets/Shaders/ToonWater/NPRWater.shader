Shader "Practical-URP/Effect/SimpleWater"
{

    Properties
    {
        [HideInInspector] _MainTex ("MainTex", 2D) = "white" { }

        _DepthGradientShallow ("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        
        // 当水面最深的时候，水的颜色
        _DepthGradientDeep ("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        
        // 水面下的最大深度，低于该值水面颜色不在发送变换。
        _DepthMaxDistance ("Depth Maximum Distance", Float) = 1
        
        // 渲染物体相交于表面所产生的泡沫颜色。
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        
        // 用来产生波浪的噪声纹理。
        _SurfaceNoise ("Surface Noise", 2D) = "white" { }
        
        // 用于控制噪音滚动速度
        _SurfaceNoiseScroll ("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)
        
        // 截止阈值，用于控制漂浮泡沫数量
        _SurfaceNoiseCutoff ("Surface Noise Cutoff", Range(0, 1)) = 0.777
        
        // 这个纹理的红色和绿色通道用来抵消噪声纹理，从而在波中产生失真。
        _SurfaceDistortion ("Surface Distortion", 2D) = "white" { }
        
        // 用这个值乘以失真。
        _SurfaceDistortionAmount ("Surface Distortion Amount", Range(0, 1)) = 0.27
        
        // 控制水面以下的距离将有助于渲染泡沫。
        _FoamMaxDistance ("Foam Maximum Distance", Float) = 0.4
        _FoamMinDistance ("Foam Minimum Distance", Float) = 0.04
    }

    SubShader
    {
        Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float4 _FoamColor;
            float _DepthMaxDistance;
            float _FoamMaxDistance;
            float _FoamMinDistance;
            float _SurfaceNoiseCutoff;
            float _SurfaceDistortionAmount;
            float2 _SurfaceNoiseScroll;
            float _FoamDistance;
        CBUFFER_END
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        TEXTURE2D(_SurfaceDistortion);
        SAMPLER(sampler_SurfaceDistortion);
        float4 _SurfaceDistortion_ST;
        
        TEXTURE2D(_SurfaceNoise);
        SAMPLER(sampler_SurfaceNoise);
        float4 _SurfaceNoise_ST;

        TEXTURE2D(_CameraDepthNormalsTexture);
        SAMPLER(sampler_CameraDepthNormalsTexture);

        struct a2v
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD;
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float4 positionSS : TEXCOORD0;
            float3 normalVS : NORMAL;
            float2 uv : TEXCOORD1;
            float2 noise_uv : TEXCOORD2;
            float2 distortion_uv : TEXCOORD3;
        };
        ENDHLSL

        pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            inline float DecodeFloatRG(float2 enc)
            {
                float2 kDecodeDot = float2(1.0, 1 / 255.0);
                return dot(enc, kDecodeDot);
            }
            
            inline float3 DecodeNormal(float4 enc)
            {
                float kScale = 1.7777;
                float3 nn = enc.xyz * float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
                float g = 2.0 / dot(nn.xyz, nn.xyz);
                float3 n;
                n.xy = g * nn.xy;
                n.z = g - 1;
                return n;
            }
            
            inline  void  DecodeDepthNormal(float4 enc, out float depth, out float3 normal)
            {
                depth = DecodeFloatRG(enc.zw);
                normal = DecodeNormal(enc);
            }

            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);
                return float4(color, alpha);
            }

            v2f vert(a2v i)
            {
                v2f o;
                ZERO_INITIALIZE(v2f, o);
                VertexPositionInputs vertexPositionInputs = GetVertexPositionInputs(i.positionOS.xyz);
                o.positionCS = vertexPositionInputs.positionCS;
                o.positionSS = ComputeScreenPos(o.positionCS);
                o.uv = i.uv;
                o.noise_uv = TRANSFORM_TEX(i.uv, _SurfaceNoise);
                o.distortion_uv = TRANSFORM_TEX(i.uv, _SurfaceDistortion);
                o.normalVS = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, i.normalOS));
                return o;
            }

            half4 frag(v2f i) : SV_TARGET
            {
                half4 depthNormalSample = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, i.positionSS.xy / i.positionSS.w);
                real depth;
                real3 normal;
                DecodeDepthNormal(depthNormalSample, depth, normal);
                depth = depth * 1000;
                float depthDifference = depth - i.positionCS.w;
                // return depthDifference;

                float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);
                // return waterColor;
                
                float2 distortSample = (SAMPLE_TEXTURE2D(_SurfaceDistortion, sampler_SurfaceDistortion, i.distortion_uv).xy * 2 - 1) * _SurfaceDistortionAmount;
                float2 noiseUV = float2((i.noise_uv.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, (i.noise_uv.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);
                float surfaceNoiseSample = SAMPLE_TEXTURE2D(_SurfaceNoise, sampler_SurfaceNoise, noiseUV).r;
                // return waterColor + surfaceNoiseSample;

                float3 normalDot = saturate(dot(normal, i.normalVS));
                // float
                // return float4(normalDot, 1);
                
                float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
                float foamDepthDifference01 = saturate(depthDifference / foamDistance);
                // return foamDepthDifference01;

                float surfaceNoiseCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;
                // return surfaceNoiseCutoff;
                // float surfaceNoise = surfaceNoiseSample > surfaceNoiseCutoff ? 1 : 0;
                // return waterColor + surfaceNoise;
                // return foamDepthDifference01;
                
                //--8.优化抗锯齿
                float surfaceNoise = smoothstep(surfaceNoiseCutoff - 0.01, surfaceNoiseCutoff + 0.01, surfaceNoiseSample);
                float4 surfaceNoiseColor = _FoamColor;
                surfaceNoiseColor.a *= surfaceNoise;
                return alphaBlend(surfaceNoiseColor, waterColor);

            }
            ENDHLSL
        }
    }
}