float2 R;
float Threshold <float uimin=0.0; float uimax=1.0;> = 0.3;
float Smooth <float uimin=0.0; float uimax=1.0;> = 0.1;
float AlphaBlur <float uimin=0.0; float uimax=1.0;> = 0.3;
bool SourceAlpha;
bool Premultiply;
bool Invert;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
#include "ColorSpace.fxh"
float4 mmap(float2 x){
	return tex2Dlod(s0,float4(x,0,(saturate(AlphaBlur)*log2(max(R.x,R.y)))));
}
float4 keyer(float4 c,float4 map,float key){
	c.a=smoothstep(Threshold-Smooth,Threshold+Smooth+.0001,key);
	if(Invert)c.a=1-c.a;
    if(Premultiply)c.rgb*=sqrt(1./c.a);
    if(SourceAlpha)c.a*=map.a;
	return c;
}
float4 pLUMA(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float4 map=mmap(x);
    return keyer(c,map,dot(map.xyz,float3(.33,.59,.11)));
}
float4 pSATUR(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float4 map=mmap(x);
    return keyer(c,map,RGBtoHSV(map.xyz).y);
}
float4 pRED(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float4 map=mmap(x);
    return keyer(c,map,c.r);
    return c;
}
float4 pGREEN(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float4 map=mmap(x);
    return keyer(c,map,c.g);
    return c;
}
float4 pBLUE(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float4 map=mmap(x);
    return keyer(c,map,c.b);
    return c;
}
float4 pALPHA(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float4 map=mmap(x);
    return keyer(c,map,c.a);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Luma{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pLUMA();}}
technique Saturation{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pSATUR();}}
technique Red{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pRED();}}
technique Green{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pGREEN();}}
technique Blue{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pBLUE();}}
technique Alpha{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pALPHA();}}
