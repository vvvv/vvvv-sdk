float2 R;
float Amount <float uimin=0.0;> =1;


texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
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

float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=1;
	float4 p=float4((x-.5)*float2(2,-2),1.1,1);
	//p.xyz=normalize(p.xyz);
	//p+=mul(float4(0,0,0,1),tVI);
	p=mul(p,tWVPI);
	p=mul(mul(p,ptV),ptP);
	//p-=mul(float4(0,0,0,1),ptVI);
	c=0;
	float2 dx=p.xy*float2(.5,-.5)+.5;
	for(float i=0;i<1;i+=1./16.){
		c+=tex2Dlod(s0,float4(lerp(x,dx-.5/R,i*Amount),0,1+8*length(x-dx)))/16.;
	}
	c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique CameraMotionBlur{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
