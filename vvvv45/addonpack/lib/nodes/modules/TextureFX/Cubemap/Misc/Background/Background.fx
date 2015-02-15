float4x4 tWVP: WORLDVIEWPROJECTION;
float4x4 tWVPI: WORLDVIEWPROJECTION;
float4x4 tW: WORLD;
float4x4 tV: VIEW;
float4x4 tP: PROJECTION;
float4x4 tWI: WORLDINVERSE;
float4x4 tVI: VIEWINVERSE;
float4x4 tPI: PROJECTIONINVERSE;
float3 posCam : CAMERAPOSITION;

float Dist <float uimin=0.0; float uimax=1.0;> =0;
texture tex0;
samplerCUBE s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float4 AmbientColor:COLOR <string uiname="Ambient Color";> = 0.0;
float4 DiffuseColor:COLOR <string uiname="Diffuse Color";> = 1.0;
//float4 DiffuseGamma:COLOR <string uiname="Diffuse Gamma";> = 0.0;
float DiffuseGamma <float uimin=0.0; float uimax=1.0;> =0.5;
//float4 AmbientGamma:COLOR <string uiname="Ambient Gamma";> = 0.0;
float3 ColGamma(float3 c,float3 a,float pv=1){
	//c.rgb=(c.rgb-.4)*(a)+.4;
	c.rgb=normalize(c.rgb)*pow(length(c.rgb)*pv,a)/pv;
	return c.rgb;
}
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=1;
	float4 p=float4((x.xy*2-1)*float2(1,-1),1,1);
	p=mul(p,tPI);
	p.xyz=normalize(p.xyz);
	p.w*=pow(1-Dist,4);
	p=mul(p,tVI);
	c=texCUBE(s0,(p));
	float4 DG=DiffuseGamma;DG=DG/(1.00001-DG);
	//c.rgb=ColGamma(c.rgb,DG,1.1);
	c.rgb=pow(c.rgb,DG);
	c=c*DiffuseColor+AmbientColor;
	//c=pow(c,pow(2,Gamma));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;vp=mul(vp,tW);}
technique Background{pass pp0{ZWriteEnable=FALSE;ZFunc=ALWAYS;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
