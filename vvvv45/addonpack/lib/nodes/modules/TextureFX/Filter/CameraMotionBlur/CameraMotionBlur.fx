float2 R;
float BlurView <float uimin=0.0;> =1;
float BlurProjection <float uimin=0.0;> =1;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};

texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
/*
float4x4 tW: WORLD;
float4x4 tV: VIEW;
float4x4 tP: PROJECTION;
float4x4 tWI: WORLDINVERSE;
float4x4 tVI: VIEWINVERSE;
float4x4 tPI: PROJECTIONINVERSE;

float4x4 tWVP: WORLDVIEWPROJECTION; 
float4x4 tWVPI: WORLDVIEWPROJECTIONINVERSE; 
float3 posCam : CAMERAPOSITION;

float4x4 ptV;
float4x4 ptP;
float4x4 ptVI;
float4x4 ptPI;
*/
float4x4 tv0;
float4x4 tv1;
float4x4 tt0;
float4x4 tt0I;
float4x4 tt1;
float4x4 tt1I;
float4 PS(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=1;
	float4 p=float4((x-.5)*float2(2,-2),1,1);
	//p.xyz=normalize(p.xyz);
	//p+=mul(float4(0,0,0,1),tVI);
	//p=mul(p,tWVPI);

	//p=mul(mul(p,ptV),ptP);
	
	p=mul(p,tt0);
	p=mul(p,tt1I);
	
	float4 p0=float4((x-.5)*float2(2,-2),1,1);
	float4 p1=float4((x-.5)*float2(2,-2),1,1);

	p0=mul(p0,tt0I);
	//p0+=-(mul(float4(0,0,0,1),tv0)-mul(float4(0,0,0,1),tv1));
	p1+=BlurProjection*.25*(mul(float4(0,0,0,1),tv0)-mul(float4(0,0,0,1),tv1));
	p1=mul(p1,tt1I);
	
	//p-=mul(float4(0,0,0,1),ptVI);
	c=0;
	
	for(float i=0;i<1;i+=1./16.){
		p=mul(lerp(p0,p1,i*BlurView),tt0);

		float2 dx=p.xy*float2(.5,-.5)/p.z+.5;
		c+=tex2Dlod(s0,float4(dx,0,1))/16.;
		
	}
	c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 PS();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 PS();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 PS();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 PS();}}
