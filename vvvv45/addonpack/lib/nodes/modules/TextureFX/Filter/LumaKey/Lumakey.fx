
float2 R;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float invert <float uimin=0.0; float uimax=1.0;> = 0.0;
float luma <float uimin=0.0; float uimax=1.0;> = 0.5;

float4 PS(float2 x:TEXCOORD0): COLOR
{
	float4 col = tex2D(s0,x) ;
	col.a=1;
	float temp= (col.r*.33)+(col.g*.59)+(col.b*.11);

	if (temp<luma)
		col.a = invert;
	else
		col.a = 1-invert;//col.a-lumaswitch ;     // Luma

	return col;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique LumaKey{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 PS();}}
