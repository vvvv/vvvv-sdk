float2 R;
float AlphaBlur <float uimin=0.0; float uimax=1.0;> = 0.1;
float4 Col:COLOR;
float sHue <float uimin=0.0; float uimax=1.0;> = 0.1;
float sSaturation <float uimin=0.0; float uimax=1.0;> = 0.1;
float sLightness <float uimin=0.0; float uimax=1.0;> = 01;
float tHue <float uimin=0.0; float uimax=1.0;> = 0.05;
float tSaturation <float uimin=0.0; float uimax=1.0;> = 0.05;
float tLightness <float uimin=0.0; float uimax=1.0;> = 0.05;
bool SourceAlpha;
bool Premultiply;
bool Invert;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
#include "ColorSpace.fxh"
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float4 map=tex2Dlod(s0,float4(x,0,(saturate(AlphaBlur)*log2(max(R.x,R.y)))));
    float3 h=RGBtoHSL(map.xyz);
    float3 k=RGBtoHSL(Col.xyz);
    
    if(!SourceAlpha)c.a=1;
    c.a*=saturate(.5+256/pow(2,sHue*10)*(tHue*.504-min(abs(h.x-k.x),min(abs(h.x-k.x-1),abs(h.x-k.x+1)))));
    c.a*=saturate(.5+256./pow(2,sSaturation*10)*(tSaturation*.504-abs(h.y-k.y)));
    c.a*=saturate(.5+256./pow(2,sLightness*10)*(tLightness*.504-abs(h.z-k.z)));
    if(Invert)c.a=1-c.a;
    if(Premultiply)c.rgb*=sqrt(1./c.a);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique ColorKey{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
