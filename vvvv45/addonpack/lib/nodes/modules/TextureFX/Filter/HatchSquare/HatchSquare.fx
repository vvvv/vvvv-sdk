float2 R;
float4 ColorA:COLOR <String uiname="Hatch Color";>  = {0, 0, 0, 1};
float4 ColorB:COLOR<String uiname="Background Color";>  = {1, 1, 1, 1};
float threshold = 5.0;
float thickness = 5.0;
float rot = 0.0;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float rand ( float2 co ){
    return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
}

float2 r2d(float2 x,float a)
{
	a*=acos(-1)*2;
	return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);
}

float4 p0(float2 vp : vpos): COLOR
{
	float2 x=(vp+0.5)/R;
	float4 col = tex2D(Samp, x);
	col.rgb = sqrt(col.rgb);
	return lerp(ColorA,ColorB,any(((r2d(vp-R*.5,rot)+R))%thickness < col.rgb * threshold))*float4(1,1,1,col.a);
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique HatchSquare{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}