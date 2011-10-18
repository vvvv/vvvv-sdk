float2 R;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);AddressU=MIRROR;AddressV=MIRROR;MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x).xyxy;
    c.xy=floor(c.xy*256)/256;
    c.zw=frac(c.zw*256);
    return c;
}
float4 p1(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x).zwzw;
    c.xy=floor(c.xy*256)/256;
    c.zw=frac(c.zw*256);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique XY{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
technique ZW{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p1();}}
