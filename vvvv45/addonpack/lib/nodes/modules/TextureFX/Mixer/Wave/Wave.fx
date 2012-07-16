float2 R;

float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;
float randomSeed;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};


float4 SampleWithBorder(float4 border, sampler2D tex, float2 uv)
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

float4 p0(float2 uv : TEXCOORD0) :COLOR
{
	float mag = 0.1;
	float phase = 14;
	float freq = 20;
	
	float2 newUV = uv + float2(mag * Fader * sin(freq * uv.y + phase * Fader), 0);
	
	float4 c1 = SampleWithBorder(0, s0, newUV);
    float4 c2 = tex2D(s1, uv);

    return lerp(c1,c2, Fader);
}


void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;AddressU[1]=CLAMP;AddressV[1]=CLAMP;vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;AddressU[1]=WRAP;AddressV[1]=WRAP;vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;AddressU[1]=MIRROR;AddressV[1]=MIRROR;vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;AddressU[1]=BORDER;AddressV[1]=BORDER;vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
