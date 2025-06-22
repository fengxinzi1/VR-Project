Shader "Custom/EquatorBandShader"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1) // 主颜色（白色）
        _BandColor ("Band Color", Color) = (1,0,0,1) // 赤道带颜色（红色）
        _BandWidth ("Band Width", Range(0, 1)) = 0.1 // 赤道带宽度
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 objectNormal : TEXCOORD0; // 使用物体空间的法线
            };
            fixed4 _MainColor;
            fixed4 _BandColor;
            float _BandWidth;
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.objectNormal = v.normal; // 直接使用物体空间的法线
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                // 计算赤道带区域
                float band = smoothstep(0.5 - _BandWidth, 0.5 + _BandWidth, abs(i.objectNormal.y));
                // 混合主颜色和赤道带颜色
                fixed4 col = lerp(_MainColor, _BandColor, band);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}