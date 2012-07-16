float2 R;
float4 ColorA:COLOR <String uiname="Hatch Color";>  = {1, 1, 1, 1};
float4 ColorB:COLOR<String uiname="Background Color";>  = {0, 0, 0, 1};
float threshold = 5.0;
float thickness = 5.0;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{
	float3 f;
	float2 x=(vp+0.5)/R;
	float3 col = tex2D(Samp, x).rgb;

	col = sqrt(col);
	if ((vp.x+vp.y)%thickness < col.r * threshold)
	{
		f = ColorA.rgb;
	}
	if ((vp.x+vp.y)%thickness < col.g * threshold)
	{
		f = ColorA.rgb;
	}
	if ((vp.x+vp.y)%thickness < col.b * threshold)
	{
		f = ColorA.rgb;
	}
	
	if ((vp.x+vp.y)%thickness < col.r * threshold)
	{
		f = ColorB.rgb;
	}
	if ((vp.x+vp.y)%thickness < col.g * threshold)
	{
		f = ColorB.rgb;
	}
	if ((vp.x+vp.y)%thickness < col.b * threshold)
	{
		f = ColorB.rgb;
	}
	return float4(f, 1.0);
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique HatchDot{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_3_0 p0();}}
