Shader "Custom/Fluid/DisplayBloomShader"
{
    Properties
    {
        _uTexture ("Texture", 2D) = "white" {}
        _Bloom ("Bloom", 2D) = "white" {}
        _Dithering ("Dithering", 2D) = "white" {}
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
		_ditherScale_x("ditherScale X", float) = 0
		_ditherScale_y("ditherScale Y", float) = 0
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

            float _texelSize_x;
            float _texelSize_y;
            sampler2D _uTexture;
            sampler2D _Bloom;
            sampler2D _Dithering;
            float _ditherScale_x;
            float _ditherScale_y;

            fixed4 frag (v2f_img i) : SV_Target
            {
                float3 C = tex2D(_uTexture, i.uv).rgb;
                float3 bloom = tex2D(_Bloom, i.uv).rgb;
                float3 noise = tex2D(_Dithering, i.uv * float2(_ditherScale_x,_ditherScale_y)).rgb;
                noise = noise * 2.0 - 1.0;
                bloom += noise / 800.0;
                float tmp = 1.0 / 2.2;
                bloom = pow(bloom.rgb, float3(tmp,tmp,tmp));
                C += bloom;
                float a = max(C.r, max(C.g, C.b));
                return float4(C, a);
            }
            ENDCG
        }
    }
}
