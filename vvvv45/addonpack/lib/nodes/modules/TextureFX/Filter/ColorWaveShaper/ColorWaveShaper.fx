float2 R;
float4 Frequency;
float4 Phase;
bool Grayscale;
bool Alpha;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 texIN(sampler s,float2 x){
    float4 c=tex2D(s,x);
    if(Grayscale)c.rgb=dot(c.rgb,normalize(float3(.33,.59,.11))/1.5);
    return c;
}
float4 pLIN(float2 x:TEXCOORD0):color{
    float4 c=texIN(s0,x);
    c=(c*Frequency+Phase);
    return c;
}
float4 pINV(float2 x:TEXCOORD0):color{
    float4 c=texIN(s0,x);float pa=c.a;
    c=1-(c*Frequency+Phase);
    if(!Alpha)c.a=pa;
    return c;
}
float4 pTRI(float2 x:TEXCOORD0):color{
    float4 c=texIN(s0,x);float pa=c.a;
    c=1-2*abs(frac((c)*Frequency+Phase)-.5);
    if(!Alpha)c.a=pa;
    return c;
}
float4 pSIN(float2 x:TEXCOORD0):color{
    float4 c=texIN(s0,x);float pa=c.a;
    c=.5+.5*cos((c*Frequency+Phase)*acos(-1)*2);
    if(!Alpha)c.a=pa;
    return c;
}
float4 pREC(float2 x:TEXCOORD0):color{
    float4 c=texIN(s0,x);float pa=c.a;
    c=step(-(frac((c*Frequency*254./255.+Phase))-.5),0);
    if(!Alpha)c.a=pa;
    return c;
}
float4 pFRA(float2 x:TEXCOORD0):color{
    float4 c=texIN(s0,x);float pa=c.a;
    c=(frac((c*Frequency*254./255.+Phase)));
    if(!Alpha)c.a=pa;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Linear{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pLIN();}}
technique Inverse{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pINV();}}
technique Triangle{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pTRI();}}
technique Sine{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pSIN();}}
technique Rectangle{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pREC();}}
technique Frac{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pFRA();}}
