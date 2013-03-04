float2 R;
bool Original=true;
bool Invert=false;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 pRED(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    float4 m=tex2D(s1,x);
    float na=m.r;
    if(Invert)na=1-na;
    if(!Original)c.a=1;
    c.a=c.a*na;
    return c;
}
float4 pGREEN(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    float4 m=tex2D(s1,x);
    float na=m.g;
    if(Invert)na=1-na;
    if(!Original)c.a=1;
    c.a*=na;
    return c;
}
float4 pBLUE(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    float4 m=tex2D(s1,x);
    float na=m.b;
    if(Invert)na=1-na;
    if(!Original)c.a=1;
    c.a*=na;
    return c;
}
float4 pALPHA(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    float4 m=tex2D(s1,x);
    float na=m.a;
    if(Invert)na=1-na;
    if(!Original)c.a=1;
    c.a*=na;
    return c;
}
float4 pLIGHTNESS(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    float4 m=tex2D(s1,x);
    float na=max(m.r,max(m.g,m.b));
    if(Invert)na=1-na;
    if(!Original)c.a=1;
    c.a*=na;
    return c;
}
float4 pSATURATION(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    float4 m=tex2D(s1,x);
    m.rgb=abs(m.rgb-m.gbr);
    float na=max(m.r,max(m.g,m.b));
    if(Invert)na=1-na;
    if(!Original)c.a=1;
    c.a*=na;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Red_Channel{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pRED();}}
technique Green_Channel{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pGREEN();}}
technique Blue_Channel{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pBLUE();}}
technique Alpha_Channel{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pALPHA();}}
technique Lightness{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pLIGHTNESS();}}
technique Saturation{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pSATURATION();}}
