Shader "Custom/Fluid/BloomPrefilterShader"
{
    Properties
    {
        _uTexture ("Texture", 2D) = "white" {}

		_curve_x("curve X", float) = 0
		_curve_y("curve Y", float) = 0
		_curve_z("curve Z", float) = 0
		_threshold("threshold", float) = 0
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

            sampler2D _uTexture;
            float _curve_x;
            float _curve_y;
            float _curve_z;
            float _threshold;

            fixed4 frag (v2f_img i) : SV_Target
            {
                float3 c = tex2D(_uTexture, i.uv).rgb;
                float br = max(c.r, max(c.g, c.b));
                float rq = clamp(br - _curve_x, 0.0, _curve_y);
                rq = _curve_z * rq * rq;
                c *= max(rq, br - _threshold) / max(br, 0.0001);
                return float4(c, 0.0);
            }
            ENDCG
        }
    }
}
