float2 R;
float Direction <float uimin=-1.0;float uimax=1.0;> = 0.25;
float Shift <float uimin=0.0;float uimax=1.0;> = 0.1;
float Hue <float uimin=0.0;float uimax=1.0;> = 0.1;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
#include "ColorSpace.fxh"
float4 sm(float4 m[16],float i){return float4(HSVtoRGB(float3(Hue,0,0)+RGBtoHSV(lerp(m[floor(i)],m[ceil(i)],frac(i)).xyz)),1);}
float4 ts(sampler s,float2 x,float2 off){float2 dir=sin((Direction+float2(0,.25))*acos(-1)*2);x+=dir*off;return float4(HSVtoRGB(float3(Hue,0,0)+RGBtoHSV(tex2D(s,x).xyz)),1);}
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);float pa=c.a;
    float sh=Shift*tex2D(s1,x).x;
    c.r=ts(s0,x,sh*.1).r;
    c.g=ts(s0,x,sh*.0).g;
    c.b=ts(s0,x,sh*-.1).b;
    c.rgb=HSVtoRGB(-float3(Hue,0,0)+RGBtoHSV(c.xyz));
    //if(Alpha)c=float4(c.rgb*c.a,pa);

    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
