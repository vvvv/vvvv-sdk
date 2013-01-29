float2 R;
float Threshold <float uimin=0.0;float uimax=1.0;> = 0.05;
float Softness <float uimin=0.0;float uimax=1.0;> = 0.05;
bool Invert;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 pic=tex2D(s0,x);
	float4 bak=tex2D(s1,x);
	float4 c=tex2D(s0,x);
	c.a=smoothstep(Threshold,Threshold+Softness+.000001,length(pic.rgb-bak.rgb));
	if(Invert)c.a=1-c.a;
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique BackgroundSubtraction{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
