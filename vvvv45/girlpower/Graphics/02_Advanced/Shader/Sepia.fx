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

// rgb to sepia:
float4 PS_RGB_to_SEPIA(float2 cTex: TEXCOORD0): COLOR0
{
   float4 currFrameSample = tex2D(Samp, cTex);
   float4 currFrameSampleYIQ;

   float4x4 YIQMatrix = { 	
		       	  0.299,  0.587,  0.114,  0,
			  0.596, -0.275, -0.321,  0,
			  0.212, -0.523,  0.311,  0,
			  1,      0,      0,      0
            	         };
         	
   float4x4 inverseYIQ ={	
			  1.0000000000000000000,  0.95568806036115671171,  0.61985809445637075388,  0,
			  1.0000000000000000000, -0.27158179694405859326, -0.64687381613840131330,  0,
		          1.0000000000000000000, -1.1081773266826619523,   1.7050645599191817149,   0,
  			  1,			  1,			   1,			    1
                        };

   // convert to YIQ space
   currFrameSampleYIQ = mul(YIQMatrix , currFrameSample);
   currFrameSampleYIQ.y = 0.2; // convert YIQ color to sepia tone
   currFrameSampleYIQ.z = 0.0;
   // convert back to RGB
   float4 res = mul( inverseYIQ, currFrameSampleYIQ);
   res.a = 1.0;
   return res;
}

// --------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------


technique TRGB_to_Sepia
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS_RGB_to_SEPIA();
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


