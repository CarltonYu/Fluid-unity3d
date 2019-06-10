Shader "Custom/Fluid/BloomBlurShader"
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
                float4 sum = float4(0.0,0.0,0.0,0.0);
                vL = i.uv - float2(_texelSize_x, 0.0);
                vR = i.uv + float2(_texelSize_x, 0.0);
                vT = i.uv + float2(0.0, _texelSize_y);
                vB = i.uv - float2(0.0, _texelSize_y);
                sum += tex2D(_uTexture, vL);
                sum += tex2D(_uTexture, vR);
                sum += tex2D(_uTexture, vT);
                sum += tex2D(_uTexture, vB);
                sum *= 0.25;
                return sum;
            }
            ENDCG
        }
    }
}
