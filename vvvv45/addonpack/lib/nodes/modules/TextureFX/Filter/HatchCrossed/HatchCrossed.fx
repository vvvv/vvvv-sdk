float2 R;
float4 ColorA:COLOR <String uiname="Background Color";>  = {1, 1, 1, 1};
float4 ColorB:COLOR<String uiname="Hatch Color";>  = {0, 0, 0, 1};
float lum_threshold_1;
float lum_threshold_2;
float lum_threshold_3;
float lum_threshold_4;
int density <float uimin=0;String uiname="Density";> = 12;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{	
	float2 x=(vp+0.5)/R;
	float lum = length(tex2D(Samp, x).rgb);
	float4 tc = ColorA;
	if (lum < lum_threshold_1)
	{
		if ((vp.x+vp.y)%density == 0.0)
		{
			tc = ColorB;
		}
	}	
	if (lum < lum_threshold_2)
	{
		if ((vp.x-vp.y)%density == 0.0)
		{
			tc = ColorB;
		}
	}	
	if (lum < lum_threshold_3)
	{
		if ((vp.x+vp.y-5.0)%density == 0.0)
		{
			tc = ColorB;
		}
	}	
	if (lum < lum_threshold_4)
	{
		if ((vp.x-vp.y-5.0)%density == 0.0)
		{
			tc = ColorB;	
		}
	}	
	return lerp (ColorB, ColorA, tc);
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique HatchCrossed{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
