// perform sobel edge detection algorithm on given texture

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
int TexHeight <string uiname="Texture Heihgt";> = 512;

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

// sobel edge detection:

//*********************************************************************************************
//
// float4 sobelEdgeDetection(float4 texCoord, sampler texture0, float width, float height)
//
// does:	Edge Dedection	
//
// gets: 	texCoords: 	Texture Coordinates
//		texture0:	texture to sample from
//		width:	width of texture
//		height:	height of texture
//
// returns:	color
//
//*********************************************************************************************

float4 sobelEdgeDetection(float2 cTex, sampler sTex, float width, float height)
{
	width  = 1 /float (width/2.0);
	height = 1 /float (height/2.0);

	float2 sampleOffsets[8] ={
					-width,	-height, 	// upper row
					 0.0,		-height,	 	
					 width,	-height,	
					-width,	 0.0,		// center row
					 width,	 0.0,
					-width,	 height,	// bottom row
					 0.0,		 height,
					 width,	 height,  	
				};


	int i =0;
	float4 c = .5;
	float2 texCoords;
	float4 texSamples[8];
	float4 vertGradient;
	float4 horzGradient;

	
	for(i =0; i < 8; i++)
	{
		texCoords = cTex + sampleOffsets[i]; // add sample offsets stored in c10-c17 (inclusive)
		// take sample
		texSamples[i] = tex2D(sTex, texCoords);
		// convert to b&w
		texSamples[i] = dot(texSamples[i], .333333f);
	}
	
	// VERTICAL Gradient
	vertGradient = -(texSamples[0] + texSamples[5] + 2*texSamples[3]);
	vertGradient += (texSamples[2] + texSamples[7] + 2*texSamples[4]);
	// Horizontal Gradient
	horzGradient = -(texSamples[0] + texSamples[2] + 2*texSamples[1]);
	horzGradient += (texSamples[5] + texSamples[7] + 2*texSamples[6]);
	
	// we could approximate by adding the abs value..but we have the horse power

	c = sqrt( horzGradient*horzGradient + vertGradient*vertGradient );
	return c;

}

float4 PSSobel(float2 cTex: TEXCOORD0): COLOR0
{
   float4 c = sobelEdgeDetection(cTex, Samp, float(TexWidth), float(TexHeight));
   return c;
}


// --------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------


technique TSobelEdgeDetection
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PSSobel();
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

