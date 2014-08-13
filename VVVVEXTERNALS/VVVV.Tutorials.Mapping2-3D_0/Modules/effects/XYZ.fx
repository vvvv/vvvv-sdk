//elliot woz 'ere
// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tVP: VIEWPROJECTION;

//the data structure: vertexshader to pixelshader
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
    float4 Pos : POSITION;
    float4 PosW : TEXCOORD0;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------

vs2ps VS(
    float4 Pos : POSITION)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    //transform position
    Out.PosW = mul(Pos, tW);
	Out.Pos = mul(Out.PosW, tVP);
    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float3 minimum = float3(-16.0f, -16.0f, -16.0f);
float3 maximum = float3(16.0f, 16.0f, 16.0f);

float4 PS(vs2ps In): COLOR
{
	In.PosW /= In.PosW.w;
	float3 xyz = In.PosW.xyz;
	float4 col;
	col.xyz = (xyz - minimum) / (maximum - minimum);
	col.a = 1.0f;
	return col;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TXYZ
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}