float2 R;
float time;
float Deepness = 0.0;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{
	float2 p = -1.0 + 2.0 * vp.xy / R.xy;
	float2 uv;
	float a = atan2(p.y,p.x);
	float r = sqrt(dot(p,p))+(Deepness/2.0);
	uv.x = .5*time+.5/r;
	uv.y = a/(3.1416);
	float3 col =  tex2D(Samp,uv).xyz;
	return float4(col*r,1.0);
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique Tunnel {pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_3_0 p0();}}
