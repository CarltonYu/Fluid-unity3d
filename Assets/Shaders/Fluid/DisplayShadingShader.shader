Shader "Custom/Fluid/DisplayShadingShader"
{
    Properties
    {
        _uTexture ("Texture", 2D) = "white" {}
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
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
            sampler2D _uTexture;

            fixed4 frag (v2f_img i) : SV_Target
            {
                vL = i.uv - float2(_texelSize_x, 0.0);
                vR = i.uv + float2(_texelSize_x, 0.0);
                vT = i.uv + float2(0.0, _texelSize_y);
                vB = i.uv - float2(0.0, _texelSize_y);
                float3 L = tex2D(_uTexture, vL).rgb;
                float3 R = tex2D(_uTexture, vR).rgb;
                float3 T = tex2D(_uTexture, vT).rgb;
                float3 B = tex2D(_uTexture, vB).rgb;
                float3 C = tex2D(_uTexture, i.uv).rgb;

                float dx = length(R) - length(L);
                float dy = length(T) - length(B);

                float3 n = normalize(float3(dx, dy, length(float2(_texelSize_x,_texelSize_y))));
                float3 l = float3(0.0, 0.0, 1.0);

                float diffuse = clamp(dot(n, l) + 0.7, 0.7, 1.0);
                C.rgb *= diffuse;

                float a = max(C.r, max(C.g, C.b));
                return float4(C, a);
            }
            ENDCG
        }
    }
}
