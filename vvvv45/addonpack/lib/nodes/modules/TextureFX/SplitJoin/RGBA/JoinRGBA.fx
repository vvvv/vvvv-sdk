//@author: vvvv group
//@help: this is a very basic template. use it to start writing your own effects. if you want effects with lighting start from one of the GouraudXXXX or PhongXXXX effects
//@tags:
//@credits:

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;

//texture
texture TexR <string uiname="Texture Red";>;
sampler SampR = sampler_state    //sampler for doing the texture-lookup
{
	Texture   = (TexR);          //apply a texture to the sampler
	MipFilter = LINEAR;         //sampler states
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

texture TexG <string uiname="Texture Green";>;
sampler SampG = sampler_state    //sampler for doing the texture-lookup
{
	Texture   = (TexG);          //apply a texture to the sampler
	MipFilter = LINEAR;         //sampler states
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

texture TexB <string uiname="Texture Blue";>;
sampler SampB = sampler_state    //sampler for doing the texture-lookup
{
	Texture   = (TexB);          //apply a texture to the sampler
	MipFilter = LINEAR;         //sampler states
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

texture TexA <string uiname="Texture Alpha";>;
sampler SampA = sampler_state    //sampler for doing the texture-lookup
{
	Texture   = (TexA);          //apply a texture to the sampler
	MipFilter = LINEAR;         //sampler states
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

//the data structure: "vertexshader to pixelshader"
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
	float4 Pos  : POSITION;
	float2 TexCd : TEXCOORD0;
};


// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 PS(vs2ps In): COLOR
{
	float4 colR = tex2D(SampR, In.TexCd);
	float4 colG = tex2D(SampG, In.TexCd);
	float4 colB = tex2D(SampB, In.TexCd);
	float4 colA = tex2D(SampA, In.TexCd);
	float4 colRGBA = 0;
	colRGBA.r = colR;
	colRGBA.g = colG;
	colRGBA.b = colB;
	colRGBA.a = colA;	
	
	return colRGBA;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TSimpleShader
{
	pass P0
	{
		//Wrap0 = U;  // useful when mesh is round like a sphere
		VertexShader = null;
		PixelShader  = compile ps_2_0 PS();
	}
}