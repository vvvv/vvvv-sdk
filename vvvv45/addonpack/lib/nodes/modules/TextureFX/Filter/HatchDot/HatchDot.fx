float2 R;
float4 ColorA:COLOR <String uiname="Dot Color";>  = {1, 1, 1, 1};
float4 ColorB:COLOR<String uiname="Background Color";>  = {0, 0, 0, 1};
float threshold =10.0;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{
	float3 f;
	float2 x=(vp+0.5)/R;
	float4 col = tex2D(Samp, x);
	col.rgb = sqrt(col.rgb);
	return lerp(ColorA, ColorB, any(((vp.x * 5.0 + vp.y)%8.0 < col.b * threshold))*float4(1,1,1,col.a));
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique HatchDot{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
