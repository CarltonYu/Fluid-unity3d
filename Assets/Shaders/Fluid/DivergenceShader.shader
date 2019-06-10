Shader "Custom/Fluid/DivergenceShader"
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
                float L = tex2D(_Velocity, vL).x;
                float R = tex2D(_Velocity, vR).x;
                float T = tex2D(_Velocity, vT).y;
                float B = tex2D(_Velocity, vB).y;

                float2 C = tex2D(_Velocity, i.uv).xy;
                if (vL.x < 0.0) { L = -C.x; }
                if (vR.x > 1.0) { R = -C.x; }
                if (vT.y > 1.0) { T = -C.y; }
                if (vB.y < 0.0) { B = -C.y; }

                float div = 0.5 * (R - L + T - B);
                return float4(div, 0.0, 0.0, 1.0);
            }
            ENDCG
        }
    }
}
