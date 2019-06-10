Shader "Custom/Fluid/PressureShader"
{
    Properties
    {
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
        
        _Divergence ("Divergence", 2D) = "white" {}
        _Pressure ("Pressure", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        // No culling or depth
        Cull Off ZWrite Off ZTest Off
        Blend [_SrcBlend][_DstBlend]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            float2 vL;
            float2 vR;
            float2 vT;
            float2 vB;
            float _texelSize_x;
            float _texelSize_y;
            sampler2D _Divergence;
            sampler2D _Pressure;
            float2 boundary (float2 uv) {
                return uv;
            }
            fixed4 frag (v2f_img i) : SV_Target
            {
                vL = i.uv - float2(_texelSize_x, 0.0);
                vR = i.uv + float2(_texelSize_x, 0.0);
                vT = i.uv + float2(0.0, _texelSize_y);
                vB = i.uv - float2(0.0, _texelSize_y);
                float L = tex2D(_Pressure, boundary(vL)).x;
                float R = tex2D(_Pressure, boundary(vR)).x;
                float T = tex2D(_Pressure, boundary(vT)).x;
                float B = tex2D(_Pressure, boundary(vB)).x;
                float C = tex2D(_Pressure, i.uv).x;
                float divergence = tex2D(_Divergence, i.uv).x;
                float pressure = (L + R + B + T - divergence) * 0.25;
                return float4(pressure, 0.0, 0.0, 1.0);
            }
            ENDCG
        }
    }
}
