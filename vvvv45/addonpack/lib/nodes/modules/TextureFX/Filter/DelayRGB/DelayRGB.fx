float2 R;
float Count=1;
float Index=0;
float Red;
float Green;
float Blue;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
	c.r=tex2D(s0,x).r*smoothstep(1,0,abs(Index-Red));
	c.g=tex2D(s0,x).g*smoothstep(1,0,abs(Index-Green));
	c.b=tex2D(s0,x).b*smoothstep(1,0,abs(Index-Blue));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique DelayRGB{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
