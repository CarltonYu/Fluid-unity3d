Shader "Custom/Fluid/ColorShader"
{
    Properties
    {
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
        _color("color", Color) = (1,1,1,1)
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
            float4 _color;

            fixed4 frag (v2f_img i) : SV_Target
            {
                return _color;
            }
            ENDCG
        }
    }
}
