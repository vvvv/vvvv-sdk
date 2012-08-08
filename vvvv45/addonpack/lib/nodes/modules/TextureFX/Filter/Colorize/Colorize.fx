float2 R;
float4 ColorA:COLOR;
float4 ColorB:COLOR;
float4 ColorC:COLOR;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{
	float2 x=(vp+0.5)/R;
	float3 pixcol = length(tex2D(Samp, x).rgb);
	float3 tc = float3 (1.0, 0.0, 0.0);
	float3 colors[3];
	colors[0] = float3 (ColorA.r, ColorA.g, ColorA.b);
	colors[1] = float3 (ColorB.r, ColorB.g, ColorB.b);
	colors[2] = float3 (ColorC.r, ColorC.g, ColorC.b);	
	float lum = (pixcol.r+pixcol.g+pixcol.b)/3.0;
	float ix;	
	if (lum < 0.5)
	{
		ix = 0;
	}
	else if (lum > 0.5)
	{
		ix = 1;
	}
	tc = lerp(colors[ix], colors[ix+1], (lum-float(ix)*0.5)/0.5);
    return float4(tc, 1.0);
}
void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique Colorize{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_3_0 p0();}}
