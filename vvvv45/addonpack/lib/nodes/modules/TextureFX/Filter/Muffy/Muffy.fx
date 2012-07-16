//Credit: Digital Slaves

float2 R;
float RingAmount = 5.0;
float RingSize = 5.0;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{
	float2 x=(vp+0.5)/R;
	float4 col = tex2D(Samp, x + ((sin(x.y * RingSize)/RingAmount ) * cos (x.x)) + (sin(x.x * RingSize)/RingAmount));
	return float4(col);
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique Muffy {pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
