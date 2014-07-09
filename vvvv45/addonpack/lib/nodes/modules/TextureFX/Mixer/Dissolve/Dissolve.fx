float2 R;
float randomSeed;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;

texture tex0,tex1,tex2;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0 (float2 uv : TEXCOORD0) : COLOR
{
	float noise = tex2D(s2, uv + randomSeed);
	if(noise > Fader)
	{
		return tex2D(s0, uv);
    }
    else
    {
		return tex2D(s1, uv);
	}
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Dissolve {pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
