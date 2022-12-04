﻿Shader "Unity Shaders/PSS" {
	Properties {
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_RampTex ("Ramp Tex", 2D) = "white" {}
		_Specular ("Specular", Color) = (1, 1, 1, 1)
		_Gloss ("Gloss", Range(8.0, 256)) = 20
		_CurveFactor ("CurveFactor", Range(0, 1)) = 1
	}
	SubShader {
		Pass { 
			Tags { "RenderPipeline" = "UniversalRenderPipeline"}
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
			
			fixed4 _Color;
			sampler2D _RampTex;
			float4 _RampTex_ST;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Specular;
			float _Gloss;
			float _CurveFactor;
			
			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 mainuv : TEXCOORD1;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float2 uv : TEXCOORD2;
				float2 mainuv : TEXCOORD3;
			};
			
			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				
				o.uv = TRANSFORM_TEX(v.texcoord, _RampTex);
				o.mainuv = TRANSFORM_TEX(v.mainuv, _MainTex);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target {
				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				// Use the texture to sample the diffuse color
				fixed3 albedo = tex2D(_MainTex, i.mainuv).rgb * _Color.rgb;
				fixed halfLambert  = 0.5 * dot(worldNormal, worldLightDir) + 0.5;
				fixed cuv = saturate(_CurveFactor * (length(fwidth(i.worldNormal)) / length(fwidth(i.worldPos))));
				fixed3 diffuseColor = tex2D(_RampTex, fixed2(halfLambert, cuv)).rgb * albedo;
				
				fixed3 diffuse = _LightColor0.rgb * diffuseColor;
				
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				fixed3 halfDir = normalize(worldLightDir + viewDir);
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(worldNormal, halfDir)), _Gloss);
				
				return fixed4(ambient + diffuse + specular, 1.0);
			}
			
			ENDCG
		}
	} 
	FallBack "Specular"
}
