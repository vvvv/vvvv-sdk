float2 R;
float2 Center = 0.5;
float Radius = 0.5;
float Amount = -0.5;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{
	float2 x=(vp+0.5)/R;
	
    float2 displace = Center - x;
    float range = saturate(1 - (length(displace) / (abs(-sin(Radius) * Radius) + 0.00000001F)));
    return tex2D(Samp, x + displace * range * Amount);
}
void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique Pinch{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
