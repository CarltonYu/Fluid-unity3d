Shader "Custom/Fluid/CurlShader"
{
    Properties
    {
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
        
        _Velocity ("Velocity", 2D) = "white" {}
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

            sampler2D _Velocity;
            
            fixed4 frag (v2f_img i) : SV_Target
            {
                vL = i.uv - float2(_texelSize_x, 0.0);
                vR = i.uv + float2(_texelSize_x, 0.0);
                vT = i.uv + float2(0.0, _texelSize_y);
                vB = i.uv - float2(0.0, _texelSize_y);
                float L = tex2D(_Velocity, vL).y;
                float R = tex2D(_Velocity, vR).y;
                float T = tex2D(_Velocity, vT).x;
                float B = tex2D(_Velocity, vB).x;
                float vorticity = R - L - T + B;
                return float4(0.5 * vorticity, 0.0, 0.0, 1.0);
            }
            ENDCG
        }
    }
}
