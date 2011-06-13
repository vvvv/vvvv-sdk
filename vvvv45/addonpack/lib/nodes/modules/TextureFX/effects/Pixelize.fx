float2 R;
int2 PixelSize;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 x:TEXCOORD0):color{
    float2 vp=x*R-.25;
    float2 sz=min(max(0.5/R,PixelSize),R);
    float4 c=tex2D(s0,floor(vp/sz)*sz/R+.5/R);
    return c;
}
technique InvertColor{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 p0();}}
