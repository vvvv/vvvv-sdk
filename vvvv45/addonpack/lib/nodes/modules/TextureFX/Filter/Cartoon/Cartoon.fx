#include "ColorSpace.fxh"

texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float2 R;

float EdgeWidth <float uimin=0.0;> =1;
float EdgeBoost <float uimin=0.0;> =.25;
float ColorBoost <float uimin=0.0;> =.25;
float ShadeGamma <float uimin=0.0; float uimax=1.0;> =0.5;

float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
	float a=0;
	for(float i=0;i<1;i+=1./16.){
		float2 off=sin((i+float2(.25,0))*acos(-1)*2);
		float4 nc=tex2Dlod(s0,float4(x+off/R*EdgeWidth,0,1));
		float3 nch=RGBtoHSV(nc.xyz);
		a=max(nch.z,a);
	}
	float4 c=tex2Dlod(s0,float4(x,0,1));
	float3 ch=RGBtoHSV(c.xyz);
	a=lerp(a,ch.z,1-2*EdgeBoost);
	ch.y+=ch.z*pow(ch.y,2)*pow(9,ColorBoost);
	ch.z/=a+.0000001;
	
	
	float shd=RGBtoHSV(tex2Dlod(s0,float4(x,0,1))).z;
	ch.z*=pow(smoothstep(.2,.8,sqrt(pow(shd,ShadeGamma))),2);
	
	c.rgb=HSVtoRGB(ch);
	//c.rgb*=smoothstep(.2,.8,sqrt(pow(shd,ShadeGamma)));
	
	c.rgb=max(c.rgb,0);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Cartoon{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
