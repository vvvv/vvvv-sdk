float2 R;
float4x4 tWVP: WORLDVIEWPROJECTION;  
float4x4 tr;
float4x4 view;
texture tex0;
samplerCUBE s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;AddressW=CLAMP;};
float4 p0(float2 x:TEXCOORD0,float3 p:TEXCOORD1):color{
    float4 c=1;
	float2 uv=float2(atan2(p.z,p.x)/acos(-1)/2+.75,acos(p.y*2)/acos(-1));
	//c.rgb=sin((c.rgb/4+Parameter)*acos(-1)*2);
	c=texCUBE(s0,(mul(float4(p.x,p.y,-p.z,0),view).xyz));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,inout float3 p:TEXCOORD1){p=vp;vp=mul(vp,tWVP);}
technique PerfectSphere{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
