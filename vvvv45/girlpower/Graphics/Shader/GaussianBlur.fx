//
// example patch: girlpower\ShadeYourPixels.v4p
//

// --------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (DX9)
float4x4 tWVP: WORLDVIEWPROJECTION;
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (DX9)

//texture
texture Tex <string uiname="Texture";>;
float4x4 tTex <string uiname="Texture Transform";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //set the sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

int TexWidth <string uiname="Texture Width";> = 512;
int TexHeight <string uiname="Texture Height";> = 512;

// -------------------------------------------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// -------------------------------------------------------------------------------------------------------------------------------------

//data that ist returned by the vertexshader
struct VS_OUTPUT
{
    float4 Pos  : POSITION;
    float2 TexC : TEXCOORD0;
};

VS_OUTPUT VS(
    float4 Pos  : POSITION,
    float2 TexC : TEXCOORD)
{
    //inititalize all fields of output struct with 0
    VS_OUTPUT Out = (VS_OUTPUT)0;

    //transform position
    Pos = mul(Pos, tWVP);
    //transform texturecoordinates
    TexC = mul(TexC, tTex);

    Out.Pos  = Pos;
    Out.TexC = TexC;

    return Out;
}

// --------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------

//gaussian blur
float4 PS_GaussianBlur(float2 cTex: TEXCOORD0 ) : COLOR
{
   // to access the pixel around, take the width and height of pic, to calculate the coordinates
   // of the pixels around the actual one

   float width  = 2.0 / TexWidth;		// <- width = 1 / (width/2);
   float height = 2.0 / TexHeight;		// <- height = 1 / (height/2);

   float2 sampleOffsets[8] ={
                               -width,	-height, 	// upper row
		                0.0,	-height,	 	
		                width,	-height,	
                               -width,	 0.0,		// center row
		                width,	 0.0,
                               -width,	 height,	// bottom row
		                0.0,	 height,
                                width,	 height,  	
				};

   int i =0;
   float4 c = .5;
   float2 texCoords;
   float4 total=0;
   float multipliers[8]= {	1,2,1, // Parts of Gaussian Kernel
	                        2, 2,
                                1,2,1};
   for(i =0; i < 8; i++)
   {
      texCoords = cTex + sampleOffsets[i]; //add sample offsets
      //(inclusive)
      // take sample, mul  by kernel
      total += tex2D(Samp, texCoords) * multipliers[i];
   }
   total += 4 * tex2D(Samp, cTex);
   c = total/16.0;
   return c;
}

// --------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------

technique TGAussianBlur
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS_GaussianBlur();
    }
}

technique TFixedFunction
{
    pass P0
    {
        //transforms
        WorldTransform[0]   = (tW);
        ViewTransform       = (tV);
        ProjectionTransform = (tP);

        //texturing
        Sampler[0] = (Samp);
        TextureTransform[0] = (tTex);
        TexCoordIndex[0] = 0;
        TextureTransformFlags[0] = COUNT2;

        //shaders
        VertexShader = NULL;
        PixelShader  = NULL;
    }
}


