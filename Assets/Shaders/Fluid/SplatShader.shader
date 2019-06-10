Shader "Custom/Fluid/SplatShader"
{
    Properties
    {
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
        
		_aspectRatio("aspectRatio", float) = 0
		_radius("radius", float) = 0
        _color("color", Color) = (1,1,1,1)
        _point("point", Color) = (1,1,1,1)
        _Target ("Target", 2D) = "white" {}
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
            sampler2D _Target;
            float _aspectRatio;
            float4 _color;
            float4 _point;
            float _radius;

            fixed4 frag (v2f_img i) : SV_Target
            {
                float2 p = i.uv - _point.xy;
                p.x *= _aspectRatio;
                float3 splat = exp(-dot(p, p) / _radius) * float3(_color.x,_color.y,_color.z);
                float3 base = tex2D(_Target, i.uv).xyz;
                return float4(base + splat, 1.0);
            }
            ENDCG
        }
    }
}
