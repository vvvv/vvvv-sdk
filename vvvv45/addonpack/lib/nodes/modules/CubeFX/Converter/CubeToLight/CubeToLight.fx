float2 R;
float4x4 tWVP: WORLDVIEWPROJECTION;  
float4x4 tV: VIEW;  

texture tex0;
samplerCUBE s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
sampler s1=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};

float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float3 r(float3 p,float3 z){z*=acos(-1)*2;float3 x=cos(z),y=sin(z);return mul(p,float3x3(x.y*x.z+y.x*y.y*y.z,-x.x*y.z,y.x*x.y*y.z-y.y*x.z,x.y*y.z-y.x*y.y*x.z,x.x*x.z,-y.y*y.z-y.x*x.y*x.z,x.x*y.y,y.x,x.x*x.y));}

float3 side(float2 x){return normalize(float3(x-.5,.5));}

float4 pAMB_PRE(float2 vp:VPOS,float3 p:COLOR0):color{float2 x=(vp+.5)/R;
    float4 c=1;

	p=float3(0,0,1);
	p.yz=r2d(p.yz,-(x.y-.5)*.5);
	p.xz=r2d(p.xz,-x.x);
	c=texCUBE(s0,float4(p,1));
	c=pow(1./(.5+5.*c),13.0);
	//c=pow(c,PW);
	//c.rgb=side(x);
    return c;
}
float4 pAMB_POST(float2 vp:VPOS):color{float2 x=(vp+.5)/R;
    float4 c=tex2Dlod(s1,float4(x,0,33));

	c=(1./pow(c,1./13.0)-.5)/5.;

	c.a=1;
    return c;
}
float4 pDIFF_PRE(float2 vp:VPOS,float3 p:COLOR0):color{float2 x=(vp+.5)/R;
    float4 c=1;

	p=float3(0,0,1);
	p.yz=r2d(p.yz,-(x.y-.5)*.5);
	p.xz=r2d(p.xz,-x.x);
	c=texCUBE(s0,float4(p,1));
	c=pow(c,4.);
	//c=pow(c,PW);
	//c.rgb=side(x);
    return c;
}
float4 pDIFF_POST(float2 vp:VPOS):color{float2 x=(vp+.5)/R;
    float4 c=1;
	c=tex2Dlod(s1,float4(x,0,33));
	c=pow(c,1./4);
	//c.rgb+=pow(1./(1.+4*texCUBE(s0,p)),1);

	//c.rgb=side(x);
	c.a=1;
    return c;
}
void vcube(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,inout float4 vc:COLOR0){vc=vp;vp=mul(vp,tWVP);}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;}
technique PreAmbient{pass pp0{AlphaBlendEnable=FALSE;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pAMB_PRE();}}
technique PostAmbient{pass pp0{AlphaBlendEnable=FALSE;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pAMB_POST();}}
technique PreDiffuse{pass pp0{AlphaBlendEnable=FALSE;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pDIFF_PRE();}}
technique PostDiffuse{pass pp0{AlphaBlendEnable=FALSE;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pDIFF_POST();}}
