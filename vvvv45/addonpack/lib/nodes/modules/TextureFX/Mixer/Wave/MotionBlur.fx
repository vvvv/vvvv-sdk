float2 R;

float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;
float randomSeed;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 uv : TEXCOORD0) :COLOR
{
	float4 c1 = 0;
	int count = 26;
	float2 direction = float2(0.05, 0.05);
	float2 offset = Fader * direction;
	float2 startUV = uv - offset * 0.5;
	float2 delta = offset / (count-1);
	for(int i=0; i<count; i++)
	{
		c1 += tex2D(s0, startUV + delta*i);
	}
	c1 /= count;
    return c1;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Trans{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
