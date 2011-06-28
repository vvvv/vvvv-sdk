//@author: catweasel
//@help: draws a mesh with a constant color
//@tags: 
//@credits:
// -------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS:
// -------------------------------------------------------------------------------------------------------------------------------------

//texture
texture Tex1 <string uiname="Texture1";>;

float4x4 tTex <string uiname="Texture Transform";>;                  //Texture Transform
sampler2D Samp = sampler_state    //sampler for doing the texture-lookup
{
	Texture   = (Tex1);          //apply a texture to the sampler
	MipFilter = LINEAR;         //set the sampler states
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

float4x4 tColor  <string uiname="Color Transform";>;

//Parameters

float invert;

float luma=0.5;

// -------------------------------------------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// -------------------------------------------------------------------------------------------------------------------------------------

float4 PS_lumakey(float2 TexC: TEXCOORD0): COLOR
{
	float4 col = tex2D(Samp, TexC) ;
	col.a=1;
	float temp= (col.r*.33)+(col.g*.59)+(col.b*.11);
	
	if (temp<luma) 
		col.a = invert;
	else 
		col.a = 1-invert;//col.a-lumaswitch ;     // Luma
	
	return col;
}

// -------------------------------------------------------------------------------------------------------------------------------------
// TECHNIQUES:
// -------------------------------------------------------------------------------------------------------------------------------------

technique TSimpleShader
{
	pass P0
	{
		VertexShader = null;
		PixelShader  = compile ps_2_0 PS_lumakey();
	}
}