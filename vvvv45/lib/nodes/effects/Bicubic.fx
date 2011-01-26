//@author: tonfilm
//@help: high quality texture scaling filter
//@tags: texture, high quality, scale, filter
//@credits: Christian Sigg, ETH Zurich and Markus Hadwiger, VRVis Research Center 
// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;

//texture
texture Tex <string uiname="Texture";>;

//samplers
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

//only for the neares neighbour ps
sampler SampPoint = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = point;         //sampler states
    MinFilter = point;
    MagFilter = point;
};

//texture transformation marked with semantic TEXTUREMATRIX to achieve symmetric transformations
float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

//alpha
float Alpha <float uimin=0.0; float uimax=1.0;> = 1;

//include the bicubic texture lookup
#include <effects\Bicubic.fxh>

//the data structure: vertexshader to pixelshader
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
    float4 Pos  : POSITION;
    float2 TexCd : TEXCOORD0;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------

vs2ps VS(
    float4 PosO  : POSITION,
    float4 TexCd : TEXCOORD0
    )
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    //transform position
    Out.Pos = mul(PosO, tWVP);

    //transform texturecoordinates
    Out.TexCd = mul(TexCd, tTex);
    
    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

// PS Bicubic
float4 PSbic(vs2ps In) : COLOR
{
  float4 col = tex2Dbicubic(Samp, In.TexCd);
  
  col.a *= Alpha;
  
  return col;
}

// PS Bilinear
float4 PSlin(vs2ps In) : COLOR
{
  float4 col = tex2D(Samp, In.TexCd);
  
  col.a *= Alpha;

  return col;
}

// PS Nearest Neighbour
float4 PSnn(vs2ps In) : COLOR
{
  float4 col = tex2D(SampPoint, In.TexCd);
  
  col.a *= Alpha;

  return col;
}



technique Bicubic
{
    pass p0
    {
		VertexShader = compile vs_1_1 VS();
		PixelShader = compile ps_2_0 PSbic();
    }
}

technique Bilinear
{
    pass p0
    {
		VertexShader = compile vs_1_1 VS();
		PixelShader = compile ps_2_0 PSlin();
    }
}

technique NearestNeighbour
{
    pass p0
    {
		VertexShader = compile vs_1_1 VS();
		PixelShader = compile ps_2_0 PSnn();
    }
}