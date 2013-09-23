// this is a very basic template. use it to start writing your own effects.
// if you want effects with lighting start from one of the GouraudXXXX or PhongXXXX effects

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;

//texture
texture Texb <string uiname="Texture Before";>;
sampler Sampb = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Texb);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Wrap;

};

//texture
texture Texa <string uiname="Texture";>;
sampler Sampa = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Texa);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Wrap;
    
};

//texture
texture Tex2 <string uiname="Depth";>;
sampler SampDepth = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex2);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

//texture transformation marked with semantic TEXTUREMATRIX to achieve symmetric transformations
float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

//the data structure: "vertexshader to pixelshader"
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
    float4 TexCd : TEXCOORD0)
{
    //declare output struct
    vs2ps Out;

    //transform position
    Out.Pos = mul(PosO, tWVP);
    
    //transform texturecoordinates
    Out.TexCd = mul(TexCd, tTex);

    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

int TileNr;
float TileInfo;
float DepthFactor;

float GetDepth(float4 col)
{
  return (col.b + col.a / 255);
}

float4 PS(vs2ps In): COLOR
{
    //copy tec coords
    float2 uv = In.TexCd.xy;

    //calc slice of depth map
    uv.x = (In.TexCd.x + TileNr) * TileInfo;

    //get displacement according to depth
    float displace = GetDepth(tex2D(SampDepth, uv)) * DepthFactor;

    //add displacement to x of texture coordinate
    uv.x = In.TexCd.x + displace;

    float4 col = 0;


    if (uv.x < 0)
      col = tex2D(Sampb, uv);
    else
      col = tex2D(Sampa, uv);
    
    return col;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TSimpleShader
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
