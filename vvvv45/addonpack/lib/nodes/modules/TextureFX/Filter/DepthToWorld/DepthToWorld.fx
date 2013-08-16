float2 R;

float4x4 tVI:VIEWINVERSE;
float4x4 tPI:PROJECTIONINVERSE;
float4x4 tP:PROJECTION;
float4x4 tV:VIEW;
float3 posCam : CAMERAPOSITION;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;AddressU=WRAP;AddressV=WRAP;};
float4 PosV(sampler s,float2 uv){
	float4 p=float4(-1.0+2.0*uv.x,-1.0+2.0*uv.y,-1.0+2.0*tex2D(s,uv).x,1.0);
	p.y*=-1.0;
	p=mul(p,tPI);
	p=float4(p.xyz*2.0/p.w,1.0);
	return p;
}
float4 PosW(sampler s,float2 uv){
	return mul(PosV(s0,uv),tVI);
}
float3 NorW(sampler s,float2 uv){
	float3 w0=PosW(s0,uv);
	float3 w1=PosW(s0,uv-float2(1,0)/R);
	float3 w2=PosW(s0,uv-float2(0,1)/R);
	float3 NorW=normalize(cross(normalize(w1-w0),normalize(w2-w0)));
	return float4(NorW,1);
}
float3 NorV(sampler s,float2 uv){
	return mul(NorW(s0,uv),tV);
}
float4 pPOS_WORLD(float2 uv:TEXCOORD0):color{
    float4 c=float4(PosW(s0,uv).xyz,1);
    return c;
}
float4 pNOR_VIEW(float2 uv:TEXCOORD0):color{
	float4 c=float4(NorV(s0,uv),1);
    return c;
}
float4 pVIEW_DEPTH(float2 uv:TEXCOORD0):color{
	float4 c=float4(PosV(s0,uv).zzz,1);
    return c;
}
float4 pNOR_WORLD(float2 uv:TEXCOORD0):color{
	float4 c=float4(NorW(s0,uv),1);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique PosWorld{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pPOS_WORLD();}}
technique NormalView{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pNOR_VIEW();}}
technique NormalWorld{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pNOR_WORLD();}}
technique ViewDepth{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pVIEW_DEPTH();}}
