Shader "Custom/Fluid/VorticityShader"
{
    Properties
    {
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
        
        _Velocity ("Velocity", 2D) = "white" {}
        _Curl ("Curl", 2D) = "white" {}
		_curl("curl", float) = 0
		_dt("dt", float) = 0
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
            sampler2D _Curl;
            float _curl;
            float _dt;
            
            fixed4 frag (v2f_img i) : SV_Target
            {
                vL = i.uv - float2(_texelSize_x, 0.0);
                vR = i.uv + float2(_texelSize_x, 0.0);
                vT = i.uv + float2(0.0, _texelSize_y);
                vB = i.uv - float2(0.0, _texelSize_y);
                float L = tex2D(_Curl, vL).x;
                float R = tex2D(_Curl, vR).x;
                float T = tex2D(_Curl, vT).x;
                float B = tex2D(_Curl, vB).x;
                float C = tex2D(_Curl, i.uv).x;

                float2 force = 0.5 * float2(abs(T) - abs(B), abs(R) - abs(L));
                force /= length(force) + 0.0001;
                force *= _curl * C;
                force.y *= -1.0;

                float2 vel = tex2D(_Velocity, i.uv).xy;
                return float4(vel + force * _dt, 0.0, 1.0);
            }
            ENDCG
        }
    }
}
