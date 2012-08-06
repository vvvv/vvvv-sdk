float2 R;
float randomSeed;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;

texture tex0,tex1,tex2;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 SampleWithBorder ( float4 border, sampler2D tex, float2 uv) : COLOR
{
	if (any(saturate(uv) - uv))
	{
		return border;
	}
	else
	{
		return tex2D(tex, uv);
	}
}

float4 p0(float2 uv : TEXCOORD0) : COLOR
{
	float offset = -tex2D(s2, float2(uv.x / 2, randomSeed));
	float4 c1 = SampleWithBorder(float4(0,0,0,0), s0, float2(uv.x, uv.y + offset * Fader));
    float4 c2 = tex2D(s1, uv);

	if (c1.a <= 0.0)
		return c2;
	else
		return lerp(c1, c2, Fader);
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique DropFade{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
