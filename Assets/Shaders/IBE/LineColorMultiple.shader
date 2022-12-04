Shader "Custom/LineColor" 
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _Color1 ("Color1", Color) = (1.0, 0.0, 0.0, 1.0)
        _Color2 ("Color2", Color) = (0.0, 1.0, 0.0, 1.0)
        _Color3 ("Color3", Color) = (0.0, 0.0, 1.0, 1.0)
        _Color4 ("Color4", Color) = (1.0, 0.0, 1.0, 1.0)
        _Color5 ("Color5", Color) = (0.0, 1.0, 1.0, 1.0)
        _Color6 ("Color6", Color) = (1.0, 1.0, 0.0, 1.0)
        
        [PowerSlider(1)] _Threshold1("Threshold1", Range(0.0, 1.0)) = 0.2
        [PowerSlider(1)] _Threshold2("Threshold2", Range(0.0, 1.0)) = 0.4
        [PowerSlider(1)] _Threshold3("Threshold3", Range(0.0, 1.0)) = 0.6
        [PowerSlider(1)] _Threshold4("Threshold4", Range(0.0, 1.0)) = 0.8
        [PowerSlider(1)] _Threshold5("Threshold5", Range(0.0, 1.0)) = 0.9
    }
    
    SubShader
    {
        Pass
        { 
            Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct a2v
            {
                float4 pos: POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos: SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            fixed4 _Color4;
            fixed4 _Color5;
            fixed4 _Color6;
            float _Threshold1;
            float _Threshold2;
            float _Threshold3;
            float _Threshold4;
            float _Threshold5;
            v2f vert (a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color1 * step(i.uv.x, _Threshold1) + 
                _Color2 * step(_Threshold1, i.uv.x) * step(i.uv.x, _Threshold2) + 
                _Color3 * step(_Threshold2, i.uv.x) * step(i.uv.x, _Threshold3) + 
                _Color4 * step(_Threshold3, i.uv.x) * step(i.uv.x, _Threshold4) + 
                _Color5 * step(_Threshold4, i.uv.x) * step(i.uv.x, _Threshold5) + 
                _Color6 * step(_Threshold5, i.uv.x);
                
                return tex2D(_MainTex, i.uv) * col;
            }
            ENDCG
        }
    }
}
