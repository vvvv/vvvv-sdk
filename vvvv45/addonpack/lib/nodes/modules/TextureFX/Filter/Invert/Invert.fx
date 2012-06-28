float2 R;
float Invert;
float InvertAlpha;
float InvertRGB;
float4 Factor <float uimin=0.0; float uimax=1.0;> = 1;
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
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique InvertColor{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
