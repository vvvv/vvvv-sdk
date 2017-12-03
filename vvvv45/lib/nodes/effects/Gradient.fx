//@author: vvvv group
//@help: Draws a gradient with linear or cosine interpolation
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
float4 c1: COLOR <string uiname="Color 1";> = {0, 0, 0, 1};
float4 c2: COLOR <string uiname="Color 2";> = {1, 1, 1, 1};

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

float4 CosineInterpolate(float4 a, float4 b, float s)
{
    float ft = s * 3.1415927;
    float4 f = (1 - cos(ft)) * .5;

    return a*(1-f) + b*f;
}

float4 PSLinear(vs2ps In): COLOR
{
	float2 tc = saturate(In.TexCd);
    return lerp(c1, c2, distance(float2(tc.x, 1), tc));
}

float4 PSCosine(vs2ps In): COLOR
{
	float2 tc = saturate(In.TexCd);
    return CosineInterpolate(c1, c2, distance(float2(tc.x, 1), tc));
}


// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TGradientLinear
{
    pass P0
    {
        VertexShader = compile vs_1_0 VS();
        PixelShader  = compile ps_2_0 PSLinear();
    }
}

technique TGradientCosine
{
    pass P0
    {
        VertexShader = compile vs_1_0 VS();
        PixelShader  = compile ps_2_0 PSCosine();
    }
}