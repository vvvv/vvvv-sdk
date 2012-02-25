float2 R;
float Threshold <float uimin=-1.0; float uimax=2.0;> = 0.3;
float AntiAlias <float uimin=0.0; float uimax=1.0;> = 0.0;
bool Alpha = false;
float Dither <float uimin=0.0; float uimax=4.0;> = 0.0;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);float pa=c.a;
    float grey=dot(c.rgb,1)/3.+dot(round(vp)%2-1,.07)*Dither;
    c=grey>Threshold;
    if(!Alpha)c.a=pa;
    return c;
}
float4 p1(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float2 off=1.0*AntiAlias;
    float4 c=p0(vp)*4
    +p0(vp+float2(0,1)*off)
    +p0(vp+float2(0,-1)*off)
    +p0(vp+float2(1,0)*off)
    +p0(vp+float2(-1,0)*off);
    off/=sqrt(2);
    c/=2;
    c=c+p0(vp+float2(1,1)*off)
    +p0(vp+float2(1,-1)*off)
    +p0(vp+float2(-1,-1)*off)
    +p0(vp+float2(-1,1)*off);
    c=c/8;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique TThreshold{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}}
