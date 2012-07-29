float2 R;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;

texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0 (float2 x : TEXCOORD0) : COLOR
{
	float pixels;
	float segment_progress;
	if (Fader < 0.5)
	{
		segment_progress = 1 - Fader * 2;
	}
	else
	{		
		segment_progress = (Fader - 0.5) * 2;
	}
    pixels = 5 + 1000 * segment_progress * segment_progress;
	float2 newUV = round(x * pixels) / pixels;	
    float4 c1 = tex2D(s0, newUV);
    float4 c2 = tex2D(s1, newUV);
	float lerp_progress = saturate((Fader - 0.4) / 0.2);
	return lerp(c1,c2, lerp_progress);	
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Pixelate{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
