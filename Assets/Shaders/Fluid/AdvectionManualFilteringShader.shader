Shader "Custom/Fluid/AdvectionManualFilteringShader"
{
    Properties
    {
		_texelSize_x("texelSize X", float) = 0
		_texelSize_y("texelSize Y", float) = 0
        
        _Velocity ("Velocity", 2D) = "white" {}
        _Source ("Source", 2D) = "white" {}
		_dyeTexelSize_x("dyeTexelSize X", float) = 0
		_dyeTexelSize_y("dyeTexelSize Y", float) = 0
		_dt("dt", float) = 0
		_dissipation("dissipation", float) = 0
        
		_BlendType("BlendType", float) = 0
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

            
            sampler2D _Velocity;
            sampler2D _Source;
            float _dyeTexelSize_x;
            float _dyeTexelSize_y;
            float _dt;
            float _dissipation;
            float _BlendType;

            float4 mymix(float4 x,float4 y,float a){
                float4 q = x*(1.0-a);
                float4 w = y*a;
                return q+w;
            }
            float4 bilerp (sampler2D sam, float2 uv, float2 tsize) {
                float2 st = uv / tsize - 0.5;

                float2 iuv = floor(st);
                float2 fuv = st - floor(st);

                float4 a = tex2D(sam, (iuv + float2(0.5, 0.5)) * tsize);
                float4 b = tex2D(sam, (iuv + float2(1.5, 0.5)) * tsize);
                float4 c = tex2D(sam, (iuv + float2(0.5, 1.5)) * tsize);
                float4 d = tex2D(sam, (iuv + float2(1.5, 1.5)) * tsize);

                return mymix(mymix(a, b, fuv.x), mymix(c, d, fuv.x), fuv.y);
            }
            
            fixed4 frag (v2f_img i) : SV_Target
            {
                float2 coord = i.uv - _dt * bilerp(_Velocity, i.uv, float2(_texelSize_x,_texelSize_y)).xy * float2(_texelSize_x,_texelSize_y);
                return float4((_dissipation * bilerp(_Source, coord, float2(_dyeTexelSize_x,_dyeTexelSize_y))).xyz, 1.0);
            }
            ENDCG
        }
    }
}
