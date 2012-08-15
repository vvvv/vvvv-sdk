float2 R;
float Parameter <float uimin=0.0; float uimax=1.0;> =0;
float4x4 tWVP: WORLDVIEWPROJECTION;  
texture tex0;
samplerCUBE s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float4x4 tPost;
float4 p0(float2 vp:VPOS):color{float2 x=(vp+.5)/R;
    float4 c=1;

	float3 p=float3(0,0,1);
	p.yz=r2d(p.yz,-(x.y-.5)*.5);
	p.xz=r2d(p.xz,x.x);
	//c.rgb=sin((c.rgb/4+Parameter)*acos(-1)*2);
	p=mul(float4(p,1),tPost);
	c=texCUBE(s0,p);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;}
technique CubeToPano{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
