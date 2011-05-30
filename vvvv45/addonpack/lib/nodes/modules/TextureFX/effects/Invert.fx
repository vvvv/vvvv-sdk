float2 R;
float Invert;
float InvertAlpha;
float InvertRGB;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    if(Invert)c.rgb=1-c.rgb;
    if(InvertRGB)c.rgb-=2*(c.rgb-dot(c.rgb,1)/3.);
    if(InvertAlpha)c.a=1-c.a;

    return c;
}
technique Posterize{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 p0();}}
