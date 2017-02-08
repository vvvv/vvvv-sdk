//@author: vvvv group
//@help: Aligns the orientation of a geometry to the camera.
//@tags: billboard, view space
//@credits:

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tWV: WORLDVIEW;
float4x4 tWVP: WORLDVIEWPROJECTION;
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (EX9)

float4x4 tA <string uiname="Transform in Viewspace";>;

//material properties
float4 cAmb : COLOR <String uiname="Color";>  = {1, 1, 1, 1};
float Alpha <float uimin=0.0; float uimax=1.0;> = 1;

//texture
texture Tex <string uiname="Texture";>;

//fixed size
bool fixedSize <string uiname = "Fixed Size"; > = false;
float2 Size = float2 (0.2, 0.2);

sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;
float4x4 tColor <string uiname="Color Transform";>;

struct vs2ps
{
    float4 PosWVP: POSITION;
    float4 TexCd : TEXCOORD0;
    float3 NormV: TEXCOORD1;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------
vs2ps VS(
    float4 PosO: POSITION,
    float3 NormO: NORMAL,
    float4 TexCd : TEXCOORD0)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;
    
    //normal in view space
    Out.NormV = normalize(mul(NormO, tA));

    //WorldView position
    float4 pos = mul(float4(0, 0, 0, 1), tWV);
	
    //position
	if (fixedSize)
	{   
		// Apply Projection to the world's view position
		pos = mul (pos, tP);
		
		// Make a perspective division
		pos.xyz /= pos.w;
				
		float aX = tP[0][0];
		float aY = tP[1][1];
		float3 aspectRatio = float3 (aX, aY, 1);

		// Add the Object's position multiplied by the viewspace transform
		// to the WorldViewProjected position multiplied by the Aspect Ratio
		Out.PosWVP = float4(pos.xyz + mul(PosO * float4(Size,1,1), tA).xyz * aspectRatio, 1);
		
	}
	else
	{
		// Add the Object's position multiplied by the viewspace transform
		// to the WorldView position and then apply Projection	
		pos.xyz += mul(PosO, tA).xyz;
		Out.PosWVP  = mul(pos, tP);
	
	}
	
    Out.TexCd = mul(TexCd, tTex);
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
    return col;
}


// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique SelfAlign
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_2_0 PS();
    }
}