Shader "Unlit/URP_ScriptGrassWater"
{
    Properties
    {
	    _MainColor("Main Color", Color) = (1, 1, 1, .5) 
	    _MainTex ("Main Texture", 2D) = "white" {}
	    _BumpMap("Wave Noise", 2D) = "white" {}
	    _Speed("Wave Speed", Range(0,1)) = 0.5
	    _Amount("Wave Amount", Range(0,10)) = 0.5
	    _Height("Wave Height", Range(0,1)) = 0.5
	    _Foam("Foamline Thickness", Range(0,3)) = 0.5
        _FoamColor("Foam Color", Color) = (1, 1, 1, .5) 
        _DistortStrength("Distort strength", Range(0,1)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent+0"
        }
        
        Pass
        {
            Name "Pass"
            Tags 
            { 
                
            }
            
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZTest LEqual
            ZWrite On
 
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
            
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
 
            CBUFFER_START(UnityPerMaterial)
            half4 _MainColor;
            float _Speed;
            float _Amount;
            float _Height;
            float _Foam;
            half4 _FoamColor;
            float _DistortStrength;
            CBUFFER_END
            
            Texture2D _MainTex;
            float4 _MainTex_ST;
            
            //Texture2D _NoiseTex;
            //float4 _NoiseTex_ST;
 
            // 贴图采样器
            SamplerState smp_Point_Repeat;
            float4 _CameraColorTexture_TexelSize;//该向量是非本shader独有，不能放在常量缓冲区
            SAMPLER(_CameraColorTexture);
            //TEXTURE2D(_CameraColorTexture); SAMPLER(sampler_CameraColorTexture);
            
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            // 顶点着色器的输入
            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv :TEXCOORD0;
            };
            
            // 顶点着色器的输出
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv :TEXCOORD0;
                float4 screenPos:TEXCOORD1;
            };
 
	    // 将采样的深度贴图中的深度值转换为
            float GetLinearEyeDepth(float2 UV)
            {
                float depth = LinearEyeDepth(SampleSceneDepth(UV.xy), _ZBufferParams);
                return depth;
            }
 
            // 顶点着色器
            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                // 采样噪声贴图
//                float4 tex = SAMPLE_TEXTURE2D_LOD(_NoiseTex, smp_Point_Repeat, v.uv, 0);
                // 修改Mesh顶点坐标Y值
		        //v.positionOS.y += sin(_Time.z * _Speed + (v.positionOS.x * v.positionOS.z * _Amount * tex)) * _Height;
		        // 计算裁剪空间位置
                o.positionCS = TransformObjectToHClip(v.positionOS);
                // 计算UV值
		        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		        o.uv.x+=0.3;
		        
		        o.screenPos = ComputeScreenPos(o.positionCS);
                // 计算屏幕空间位置(此处还没有进行齐次除法)
                return o;
            }
 
            // 片段着色器
            half4 frag(Varyings i) : SV_TARGET 
            {    
                half2 speed = _Speed * _Time.y * 0.01;
                half3 bump = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv.xy + speed)).rgb;
                half2 offset = bump.xy;
                i.screenPos.xy += offset * i.screenPos.z;
 
                //return _MainColor;
                //half3 col = SAMPLE_TEXTURE2D(_CameraColorTexture,sampler_CameraColorTexture,i.uv)*5;
                half4 col = tex2D(_CameraColorTexture,i.screenPos.xy/i.screenPos.w) * _MainColor;
                //float4 col = float4(1,1,1,1);
                //col *= 5;
                return col;
                
            }
            
            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}