Shader "Custom/Fluid/BackgroundShader"
{
    Properties
    {
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
		_aspectRatio("aspectRatio", float) = 0
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

            float _aspectRatio;

            fixed4 frag (v2f_img i) : SV_Target
            {
                float2 uv = floor(i.uv * 25.0 * float2(_aspectRatio, 1.0));
                float v = uv.x + uv.y - 2.0*floor((uv.x + uv.y)/2.0);
                v = v * 0.1 + 0.8;
                return float4(v,v,v,1.0);
            }
            ENDCG
        }
    }
}
