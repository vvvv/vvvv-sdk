float2 R;
float4x4 tWVP: WORLDVIEWPROJECTION;  
texture tex0;
samplerCUBE s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float3 r(float3 p,float3 z){z*=acos(-1)*2;float3 x=cos(z),y=sin(z);return mul(p,float3x3(x.y*x.z+y.x*y.y*y.z,-x.x*y.z,y.x*x.y*y.z-y.y*x.z,x.y*y.z-y.x*y.y*x.z,x.x*x.z,-y.y*y.z-y.x*x.y*x.z,x.x*y.y,y.x,x.x*x.y));}
float3 side(float2 x){return normalize(float3(x-.5,.5));}
float4x4 tPost;
float4 p0(float2 vp:VPOS):color{float2 x=(vp+.5)/R;
    float4 c=1;
	float3 p=float3(0,0,1);
	x=float2(frac(x.x),1-x.y);
	float2 u=x*float2(4,3)-float2(floor(x.x*4),1);
	p=side(u);
	p.xz=r2d(p.xz,floor(x.x*4)/4.);
	float4 p1=float4(p,1);
	float4 p2=float4(r(side(x*float2(4,3)-float2(floor(x.x*4),0)),float3(-0.25,0.25,0)),1);
	float4 p3=float4(r(side(x*float2(4,3)-float2(floor(x.x*4),2)),float3(0.25,0.25,0)),1);
	p1=mul(p1,tPost);
	p2=mul(p2,tPost);
	p3=mul(p3,tPost);
	p1*=p1.w;
	p2*=p2.w;
	p3*=p3.w;
	
	c=0;
	c+=texCUBE(s0,p1)*(u.y>0&&u.y<1);
	c+=texCUBE(s0,p2)*(u.y<=0)*(x.x>=.25&&x.x<=.5);
	c+=texCUBE(s0,p3)*(u.y>=1)*(x.x>=.25&&x.x<=.5);

    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;}
technique CubeToCross{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
