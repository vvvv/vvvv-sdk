float2 R;
float Invert;
float InvertAlpha;
float InvertRGB;
float Factor <float uimin=0.0; float uimax=1.0;> = 1;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x),e=c;
    if(Invert)e.rgb=1-e.rgb;
    if(InvertRGB)e.rgb-=2*(e.rgb-dot(e.rgb,1)/3.);
    if(InvertAlpha)e.a=1-e.a;
    c=lerp(c,e,Factor);
    return c;
}
technique InvertColor{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 p0();}}
