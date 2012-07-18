float2 R;
float Parameter <float uimin=0.0; float uimax=1.0;> =0;
float4x4 tWVP: WORLDVIEWPROJECTION;  
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float4x4 tPost;
float4 p0(float2 x:TEXCOORD0,float3 p:TEXCOORD1):color{
    float4 c=1;

	p=mul(float4(p,1),tPost);
	float2 uv=float2(atan2(p.z,p.x)/acos(-1)/2+.75,acos(p.y*2)/acos(-1));
	//c.rgb=sin((c.rgb/4+Parameter)*acos(-1)*2);
	
	c=tex2Dlod(s0,float4(1-uv.x,uv.y,0,1));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,inout float3 p:TEXCOORD1){p=vp;vp=mul(vp,tWVP);}
technique PanoToCube{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
