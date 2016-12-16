float2 R;
float4 Levels <float uimin=1.0;> = {4.0,4.0,4.0,4.0};
bool Alpha = 0;
float4 Dithering <float uimin=0.0;float uimax=1.0;> = 0;
float Smooth <float uimin=0.0;float uimax=1.0;> = 0.1;
float4 Phase <float uimin=0.0;float uimax=1.0;> = 0;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
#include "ColorSpace.fxh"
float4 posterizer(float4 c,float2 vp){
	float4 ph=(Phase);
	c=c*max(Levels,0)+Dithering*(1-Smooth)*dot(vp%2,float2(.75,.25))*frac(c*max(Levels,0));
	c+=ph;
	return (floor(c)-ph+saturate((frac(c)-.5)/(.00001+Smooth)+.5))/(max(Levels,0)+.000000001);
}
float4 pRGB(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);float pa=c.a;
    c=posterizer(c,vp);
    if(!Alpha)c.a=pa;
    return c;
}
float4 pHSV(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);float pa=c.a;
    c.rgb=RGBtoHSV(c.rgb);
	c=posterizer(c,vp);
	c.rgb=HSVtoRGB(c.rgb);
    if(!Alpha)c.a=pa;
    return c;
}
float4 pHSL(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);float pa=c.a;
    c.rgb=RGBtoHSL(c.rgb);
	c=posterizer(c,vp);
	c.rgb=HSLtoRGB(c.rgb);
    if(!Alpha)c.a=pa;
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique RGB{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pRGB();}}
technique HSV{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pHSV();}}
technique HSL{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pHSL();}}

