float2 R;
float Parameter <float uimin=0.0; float uimax=1.0;> =0;
float4x4 tWVP: WORLDVIEWPROJECTION; 
float2 Scale=1;
float FixBorder <float uimin=0.0;float uimax=1.0;> =0.0;
float2 Offset=0;

texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;AddressW=WRAP;};
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=1;
	//
	x=x/Scale+Offset;
	float2 off=.5/R;
	if(x.y>2./3.)off.y*=-1;
	if(x.y<1./3.)off.x*=-1;
	//x=normalize(x);
	c=tex2Dlod(s0,float4(x+off,0,1));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy/=(1-FixBorder);vp=mul(vp,tWVP);}
technique CrossToCube{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
