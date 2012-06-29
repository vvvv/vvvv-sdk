float2 R;
//float Width <float uimin=0.0; float uimax=1.0;> =.5;
float LOD <float uimin=0.0; float uimax=1.0;> =0.25;
float SpotGamma =1;
float Contrast=0;
//float Fade <float uimin=0.0; float uimax=1.0;> =0.25;


float4x4 tWVP: WORLDVIEWPROJECTION;  
float4x4 tW: WORLD;  
texture tex0;
samplerCUBE s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;AddressW=CLAMP;};
texture tex1;
samplerCUBE s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;AddressW=CLAMP;};

float3 r(float3 p,float3 z){z*=acos(-1)*2;float3 x=cos(z),y=sin(z);return mul(p,float3x3(x.y*x.z+y.x*y.y*y.z,-x.x*y.z,y.x*x.y*y.z-y.y*x.z,x.y*y.z-y.x*y.y*x.z,x.x*x.z,-y.y*y.z-y.x*x.y*x.z,x.x*y.y,y.x,x.x*x.y));}
void vSPOT(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,inout float3 p:TEXCOORD1,inout float4 c:COLOR0){
		float MaxLOD=log2(max(R.x,R.y));


	float2 px=float2((uv.x-.5),(uv.y-.5))*.5;
	
	c=texCUBElod(s0,float4(normalize(mul(float4(px,.5,0),tW).xyz),1+MaxLOD*LOD));
	p=vp;vp=mul(vp,tWVP);
}
float4 pSPOT(float2 x:TEXCOORD0,float3 p:TEXCOORD1,float4 c:COLOR0):color{
	if(length(x-.5)>.5)discard;
	float MaxLOD=log2(max(R.x,R.y));
	float2 px=float2((x.x-.5),-(x.y-.5))*.05;
	c.a=1;
	c*=pow(smoothstep(.5,0,length(x-.5)),pow(2,SpotGamma));
    return c;
}
void vs3d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,inout float3 p:TEXCOORD1,inout float4 vc:COLOR0){
	vc=0;
	vc+=texCUBElod(s1,float4( 1, 0, 0,99));
	vc+=texCUBElod(s1,float4(-1, 0, 0,99));
	vc+=texCUBElod(s1,float4( 0, 1, 0,99));
	vc+=texCUBElod(s1,float4( 0,-1, 0,99));
	vc+=texCUBElod(s1,float4( 0, 0, 1,99));
	vc+=texCUBElod(s1,float4( 0, 0,-1,99));
	vc/=6.;
	p=vp;
	vp=mul(vp,tWVP);
}
float linstep(float a,float b,float x){return saturate((x-a)/(b-a));}
float mx(float3 x){return max(x.x,max(x.y,x.z));}
float4 LevelBlend(samplerCUBE s,float3 x,float lod){
	lod+=1;
	float MaxLOD=log2(max(R.x,R.y));
	float4 c=0;
	float3 cx=x/max(length(x.x),max(length(x.y),length(x.z)));
	c+=texCUBElod(s,float4(x.xyz,lod));
	float ed=(lod-1)/MaxLOD;
	ed=1-ed*ed*ed*.5;
	if(!(mx(abs(cx.xyy))<ed||mx(abs(cx.xzz))<ed||mx(abs(cx.zyy))<ed)){
		c+=texCUBElod(s,float4(x.zyx*float3( 1, 1, 1),lod))*linstep( ed, 1,cx.z)*linstep( ed, 1,cx.x);
		c+=texCUBElod(s,float4(x.zyx*float3( 1, 1, 1),lod))*linstep(-ed,-1,cx.z)*linstep(-ed,-1,cx.x);
		c+=texCUBElod(s,float4(x.zyx*float3(-1, 1,-1),lod))*linstep( ed, 1,cx.z)*linstep(-ed,-1,cx.x);
		c+=texCUBElod(s,float4(x.zyx*float3(-1, 1,-1),lod))*linstep(-ed,-1,cx.z)*linstep( ed, 1,cx.x);
		c+=texCUBElod(s,float4(x.xzy*float3( 1, 1, 1),lod))*linstep( ed, 1,cx.z)*linstep( ed, 1,cx.y);
		c+=texCUBElod(s,float4(x.xzy*float3( 1, 1, 1),lod))*linstep(-ed,-1,cx.z)*linstep(-ed,-1,cx.y);
		c+=texCUBElod(s,float4(x.xzy*float3( 1,-1,-1),lod))*linstep( ed, 1,cx.z)*linstep(-ed,-1,cx.y);
		c+=texCUBElod(s,float4(x.xzy*float3( 1,-1,-1),lod))*linstep(-ed,-1,cx.z)*linstep( ed, 1,cx.y);
		c+=texCUBElod(s,float4(x.yxz*float3( 1, 1, 1),lod))*linstep( ed, 1,cx.x)*linstep( ed, 1,cx.y);
		c+=texCUBElod(s,float4(x.yxz*float3( 1, 1, 1),lod))*linstep(-ed,-1,cx.x)*linstep(-ed,-1,cx.y);
		c+=texCUBElod(s,float4(x.yxz*float3(-1,-1, 1),lod))*linstep( ed, 1,cx.x)*linstep(-ed,-1,cx.y);
		c+=texCUBElod(s,float4(x.yxz*float3(-1,-1, 1),lod))*linstep(-ed,-1,cx.x)*linstep( ed, 1,cx.y);
	}
	c.xyz/=c.a;
	c.a=1;
	return c;
}
float4 pDIV(float2 x:TEXCOORD0,float3 p:TEXCOORD1,float4 vc:COLOR0):color{
    float4 c=texCUBElod(s0,float4(normalize(mul(float4((x.x-.5),-(x.y-.5),.5,0),tW).xyz),1));
	//c+=LevelBlend(s1,mul(float4(x.x-.5,.5-x.y,.5,0),tW).xyz,7);
	c/=c.a;
	vc.xyz*=1.2;
	c.xyz=normalize(c.xyz)*((length(c.xyz)-length(vc.xyz))*pow(2,Contrast)+length(vc.xyz));
	//c.xyz*=2;
    return c;
}




technique Spot{pass pp0{vertexshader=compile vs_3_0 vSPOT();pixelshader=compile ps_3_0 pSPOT();}}
technique DivideByAlpha{pass pp0{vertexshader=compile vs_3_0 vs3d();pixelshader=compile ps_3_0 pDIV();}}
