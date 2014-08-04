float2 R;
float Width <float uimin=0.0;> =0.5;
float Limit <float uimin=0.0;> =1;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
float4x4 tVI:VIEWINVERSE;
float4x4 tPI:PROJECTIONINVERSE;
float4x4 tP:PROJECTION;
float4x4 tV:VIEW;
float4x4 tVI_p;
float4x4 tPI_p;
float4x4 tP_p;
float4x4 tV_p;

float3 posCam : CAMERAPOSITION;
texture texCOL;
sampler sCOL=sampler_state{Texture=(texCOL);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
texture texDEP1;
sampler sDEP1=sampler_state{Texture=(texDEP1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
texture texDEP2;
sampler sDEP2=sampler_state{Texture=(texDEP2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};

float4 UVDtoXYZ(float3 x){
	float4 p=float4(-1.0+2.0*x.x,-1.0+2.0*x.y,-1.0+2.0*x.z,1.0);
	p.y*=-1.0;
	p=mul(p,tPI);
	p=float4(p.xyz*2.0/p.w,1.0);
	p=mul(p,tVI);
	return p;
}
float2 XYZtoUV(float4 p){
	p=mul(p,tV);
	p=mul(p,tP);
	p/=p.w;
	float2 uv=p.xy*float2(1,-1)*0.5+0.5;
	return uv;
}

float4 PS(float2 uv:TEXCOORD0):color{
	float z=tex2D(sDEP1,uv).x;
	float4 p=float4(UVDtoXYZ(float3(uv,tex2D(sDEP1,uv).x)).xyz,1);

	float4 pp=mul(mul(p,tV_p),tP_p);
	pp=pp/pp.w;
	float2 dx=pp.xy/float2(1,-1)*.5+0.5-0.5/R;
	float4 c=1;
	float4 p2=float4(UVDtoXYZ(float3(dx,tex2D(sDEP1,dx).x)).xyz,1);
	c=float4(tex2D(sCOL,uv).xyz,1)*.01;
	for(float i=0;i<1;i+=1./26.){
		float4 p3=lerp(p,p2,(i-.5)*Width);
		p3=mul(mul(p3,tV),tP);
		float2 dx=p3.xy/p3.w*float2(1,-1)*.5+.5;
		if(dx.x<0||dx.x>1||dx.y<0||dx.y>1)continue;
		c+=float4(tex2D(sCOL,dx).xyz,1)*smoothstep(Limit,0,length(p2.xyz-p.xyz)/z);
	}
	c.rgb/=c.a;
	c.a=tex2D(sCOL,uv).a;
    return c;
}


void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 PS();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 PS();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 PS();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 PS();}}
