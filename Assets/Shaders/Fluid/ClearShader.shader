Shader "Custom/Fluid/ClearShader"
{
    Properties
    {
        _uTexture ("Texture", 2D) = "white" {}
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
		_value("value", float) = 0
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
            float _value;

            fixed4 frag (v2f_img i) : SV_Target
            {
                return _value * tex2D(_uTexture, i.uv);
            }
            ENDCG
        }
    }
}
