float2 R;
float Parameter <float uimin=0.0; float uimax=1.0;> =0;
float4x4 tWVP: WORLDVIEWPROJECTION;  
float Blur <float uimin=0.0;float uimax=1.0;> = 1.0;
float4x4 tR;
float4 Col[6]:COLOR;
texture tex0;
samplerCUBE s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;AddressW=CLAMP;};
//float linstep(float a,float b,float x){return saturate((x-a)/(b-a));}

float4 p0(float2 x:TEXCOORD0,float3 p:TEXCOORD1):color{
    float4 c=0.00001;

//	float2 uv=float2(atan2(p.z,p.x)/acos(-1)/2+.75,acos(p.y*2)/acos(-1));
	float s2b=max(length(p.x),max(length(p.y),length(p.z)));
	float3 cx=p/s2b;
	c.rgb=0;
	
	float ed=s2b*.99*(1-Blur);
	float ea=.5;
	c+=smoothstep(ed,ea,p.x)*Col[0];
	c+=smoothstep(ed,ea,p.z)*Col[1];
	c+=smoothstep(-ed,-ea,p.x)*Col[2];
	c+=smoothstep(-ed,-ea,p.z)*Col[3];
	c+=smoothstep(ed,ea,p.y)*Col[4];
	c+=smoothstep(-ed,-ea,p.y)*Col[5];
	c.rgb/=c.a;
	c.a=1;
	/*
	//c.rgb*=0.02;
	float3 v=float3(4,2,3);
	for(float i=0;i<5;i++){
		p=mul(float4(p.xyz,1),tR);
		v=mul(v,tR);
		//c.rgb+=(length(p.xy)<.5*p.z*pow(.8,i))*pow(.75,.5*v)*2;
	}
	c.rgb=length(sin(p*888));
	c.rgb=.5-.2*c.rgb+(c.r<Blur*2)*.5;
	
	c.a=1;
    */
	return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,inout float3 p:TEXCOORD1){p=vp;vp=mul(vp,tWVP);}
technique ColorCube{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
