float2 R;
texture tex0,tex1,tex2;
sampler s0=sampler_state{Texture=(tex0);AddressU=WRAP;AddressV=WRAP;MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);AddressU=WRAP;AddressV=WRAP;MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
#define PW (13.)
#define kk (.8);
float4 AV0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
	c.rgb*=c.a;
    return c;
}
float4 AV1(float2 x:TEXCOORD0):color{
	float4 c=tex2Dlod(s0,float4(x,0,33));
	c.rgb/=c.a;
    return c;
}
float4 MX0(float2 x:TEXCOORD0):color{
    float4 c=pow(tex2D(s0,x),PW)*kk;
    return c;
}
float4 MX1(float2 x:TEXCOORD0):color{
    float4 mc=pow(tex2Dlod(s0,float4(x,0,33)),1./PW)/kk;
	float4 c=pow(tex2D(s0,x),1./PW);
	c=mc;
	//if(x.x>.1)c=c>mc;
	//if(x.x>.9)c=mc;
	//c.a=1;
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique TAverage{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 AV0();}pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 AV1();}}
technique TMax{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 MX0();}pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 MX1();}}
