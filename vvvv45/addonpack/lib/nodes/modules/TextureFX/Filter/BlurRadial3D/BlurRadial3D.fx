float2 R;
float Zoom <float uimin=0.0; float uimax=1.0;> =0;
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
float3 Point;
int Iter <float uimin=0.0;> =16;
float Fade <float uimin=0.0;float uimax=1.0;> =0.5;
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
	float4 pp=mul(mul(mul(float4(Point.xyz,1),tW),tV),tP);
	float2 cx=pp.xy*.5*float2(1,-1)/pp.z+.5;
	c=0;
	for(float i=0;i<1;i+=1./min(Iter,80)){
		float2 dx=(x-cx)*(1-i*Zoom)+cx;
		c+=tex2Dlod(s0,float4(dx,0,1))/min(Iter,80)*lerp(1,2*smoothstep(1,0,i),Fade);
	}
	//if(length(x-cx)<.01)c.rgb=float3(1,0,0);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique RadialBlur3D{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
