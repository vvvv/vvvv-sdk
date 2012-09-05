float2 R;
float4x4 tVI:VIEWINVERSE;
float4x4 tPI:PROJECTIONINVERSE;
float4x4 tP:PROJECTION;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float4 p0(float2 uv:TEXCOORD0):color{
    float4 c=tex2D(s0,uv);
	float4 p=float4(-1.0+2.0*uv.x,-1.0+2.0*uv.y,-1.0+2.0*tex2D(s0,uv).x,1.0);
	p.y*=-1.0;
	p=mul(p,tPI);
	p=float4(p.xyz*2.0/p.w,1.0);
	p=mul(p,tVI);
	c.rgb=p;
	c.a=1.0;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique DepthToWorld{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
