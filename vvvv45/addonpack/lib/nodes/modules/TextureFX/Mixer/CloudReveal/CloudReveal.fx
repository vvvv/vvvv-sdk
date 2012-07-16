float2 R;

float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;
texture tex0,tex1,tex2;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 uv : TEXCOORD0) :COLOR
{
	float cloud = tex2D(s2, uv);
    float4 c1 = tex2D(s0, uv);
    float4 c2 = tex2D(s1, uv);
	float a;
	
	if (Fader < 0.5)
	{
		a = lerp(0.0, cloud, Fader / 0.5);
	}
	else
	{
		a = lerp(cloud, 1.0, (Fader - 0.5) / 0.5);
	}
	
    return (a < 0.5) ? c1 : c2;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique CloudReveal{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
