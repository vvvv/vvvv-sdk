float2 R;
float randomSeed = 0.0;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;

texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 uv : TEXCOORD0) : COLOR
{
	float offset = min(Fader+Fader*tex2D(s0, float2(uv.x, randomSeed)).r, 1.0);
	uv.y -= offset;
	if(uv.y > 0.0)
	{
		return tex2D(s0, uv);
	}
	else
	{
		return tex2D(s1, frac(uv));
	}
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Blood{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
