//@author: vvvv group
//@help: Draws an object in Projection Space in pixel units using the Constant Shader.
//@tags: hlsl, pixel, projection space
//@credits:

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        	//the models world matrix
float4x4 tV: VIEW;         	//view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;	//projection matrix as set via Renderer (EX9)
float4x4 tWVP: WORLDVIEWPROJECTION;
float2 invTargetSize: INVTARGETSIZE; // Inverse Renderer Size in px
float2 targetSize: TARGETSIZE; // Inverse Renderer Size in px

//Additional Transform in Projection Space
float4x4 tA <string uiname="Transform in Projection Space (px)";>;

//material properties
float4 cAmb : color <String uiname="Color";>  = {1, 1, 1, 1};
float Alpha = 1;

//Texture
texture Tex <string uiname="Texture";>;

//Texture Transform with semantic TEXTUREMATRIX to achieve symmetric transformations
float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

//Color Transform
float4x4 tColor <string uiname="Color Transform";>;

int VPCount: VIEWPORTCOUNT;
int VPIndex: VIEWPORTINDEX;
int ActiveVPIndex = -1;

sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
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
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------
vs2ps VS(
    float4 PosO  : POSITION,
    float4 TexCd : TEXCOORD0)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

     //transform texturecoordinates
    Out.TexCd = mul(TexCd, tTex);

	// Aspect Ratio
	float3 aspectRatio;
	float coeff = targetSize.y / targetSize.x; // width / height
	
	if (coeff >= 1)
	{
		aspectRatio = float3 (coeff, 1, 1);
	}
	else
	{
		aspectRatio = float3 (1, 1/coeff, 1);
	}
	
	//World position
    float4 pos = mul(float4(0, 0, 0, 1), tW);
	
	//Corrected WorldView Position
	float3 worldPosition = pos.xyz * aspectRatio;
		
	//Apply Additional Transform in Projection Space
	float4 PosInProjection = mul(PosO, tA);
	
	//Calculate Pixel Size coeff from Renderer Size (in px)
	float3 pixelSizeCoeff = float3(2 * invTargetSize,1);
	
	//Adjust the position according to the Pixel Size Coeff
	float3 vertexPos = PosInProjection.xyz * pixelSizeCoeff;
	
	//Final Vertex Position
	Out.Pos = float4(worldPosition + vertexPos, 1);
		
    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 PS(vs2ps In): COLOR
{
    float4 col = tex2D(Samp, In.TexCd) * cAmb;
    col = mul(col, tColor);
	col.a *= Alpha;
	
	if ((ActiveVPIndex >= 0) && (ActiveVPIndex != VPIndex))
		col.a = 0;
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
        //Wrap0 = U;  // useful when mesh is round like a sphere
        
        Lighting       = FALSE;

        //shaders
        VertexShader = NULL;
        PixelShader  = NULL;
    }
}
