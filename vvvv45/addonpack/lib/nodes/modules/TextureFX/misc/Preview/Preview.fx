float2 R;
float3 VR;
float4x4 tWVP: WORLDVIEWPROJECTION;  
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
texture tex1;
samplerCUBE sCUBE=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float4x4 tV;
float4x4 tVI;
float4x4 tP;
float4x4 tPI;
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float3 r(float3 p,float3 z){z*=acos(-1)*2;float3 x=cos(z),y=sin(z);return mul(p,float3x3(x.y*x.z+y.x*y.y*y.z,-x.x*y.z,y.x*x.y*y.z-y.y*x.z,x.y*y.z-y.x*y.y*x.z,x.x*x.z,-y.y*y.z-y.x*x.y*x.z,x.x*y.y,y.x,x.x*x.y));}

float3 side(float2 x){return normalize(float3(x-.5,.5));}
int CH=0;
int Type=0;
#include "ColorSpace.fxh"
float4 ShowChannels(float4 c){
	switch(CH){
		case 0: {return c;break;}
		case 1: {return float4(c.rgb,1);break;}
		case 2: {return float4(c.rrr,c.a);break;}
		case 3: {return float4(c.ggg,c.a);break;}
		case 4: {return float4(c.bbb,c.a);break;}
		case 5: {return float4(c.aaa,1);break;}
		case 6: {return float4(RGBtoHSL(c.rgb).xxx,1);break;}
		case 7: {return float4(RGBtoHSL(c.rgb).yyy,1);break;}
		case 8: {return float4(RGBtoHSL(c.rgb).zzz,1);break;}
		case 9: {return float4(RGBtoHSV(c.rgb).zzz,1);break;}
	}
	return c;
}
float4 pCROSS(float2 x:TEXCOORD0):color{
    float4 c=1;
	x=float2(frac(x.x),1-x.y);
	float2 u=x*float2(4,3)-float2(floor(x.x*4),1);
	float3 p=side(u);
	p.xz=r2d(p.xz,floor(x.x*4)/4.);
	c=0;
	p=mul(p.xyz,tV);
	c+=texCUBE(sCUBE,p)*(u.y>0&&u.y<1);
	p=r(side(x*float2(4,3)-float2(floor(x.x*4),0)),float3(-0.25,0.25,0));
	p=mul(p.xyz,tV);
	c+=texCUBE(sCUBE,p)*(u.y<=0)*(x.x>=.25&&x.x<=.5);
	p=r(side(x*float2(4,3)-float2(floor(x.x*4),2)),float3(0.25,0.25,0));
	p=mul(p.xyz,tV);
	c+=texCUBE(sCUBE,p)*(u.y>=1)*(x.x>=.25&&x.x<=.5);
    return ShowChannels(c);
}
float4 pPANO(float2 x:TEXCOORD0):color{
    float4 c=1;
	float3 p=float3(0,0,1);
	p.yz=r2d(p.yz,-(x.y-.5)*.5);
	p.xz=r2d(p.xz,x.x);
	p=mul(p.xyz,tV);
	c=texCUBE(sCUBE,p);
    return ShowChannels(c);
}
float4 pBALL(float2 x:TEXCOORD0):color{
    float4 c=1;
	float3 p=float3(0,0,1);
	//x=(x-.5)*mul(float4(1,1,0,0),tPI).y*length(mul(float4(0,0,0,1),tVI).xyz)*1.39+.5+mul(float4(0,0,0,1),tP).xy;
	p.yz=r2d(p.yz,-(x.y-.5)*.5);
	p.xz=r2d(p.xz,-x.x);
	float2 dx=(x-.5)*R/min(R.x,R.y);
	//p=float3(0,0,1);
	p.xy=normalize(dx)*sin(length(dx)*acos(-1));
	p.z=cos(length(dx)*acos(-1));
	
	//p.xz=r2d(p.xz,-dx.x);
	p=reflect(float3(0,0,1),p);
	p=mul(p.xyz,tV);
	c=texCUBE(sCUBE,p)*smoothstep(.5,.4999-2*fwidth(length(dx)),length(dx));
    return ShowChannels(c);
}
float4 pTEX2D(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    return ShowChannels(c);
}
float4 pVOL(float2 x:TEXCOORD0):color{
	float2 sz=floor(sqrt(VR.z));
	float2 fx=frac(x*sz);
	float z=(floor(x.x*sz.x)+floor(x.y*sz.y)*sz.x)/sz.x/sz.y;
  	float4 c=tex3D(s0,float4(fx,z,1));
    return ShowChannels(c);
}
float4 pRED(float2 x:TEXCOORD0):color{

  	float4 c=pTEX2D(x);
	
	if(Type==0)c=pTEX2D(x);
	
    return ShowChannels(c);
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=1;vp=mul(vp,tWVP);}

technique Ball{
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pTEX2D();}
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pBALL();}
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pVOL();}
}
technique Panorama{
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pTEX2D();}
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pPANO();}
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pVOL();}
}
technique Cross{
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pTEX2D();}
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pCROSS();}
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pVOL();}
}
