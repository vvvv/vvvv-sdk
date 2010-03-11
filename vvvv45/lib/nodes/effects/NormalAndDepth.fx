//credits go to Rubicon (of gamedev.net) for the trick of packing normals + depth into one RGBA texture which i took from his implementation:
//http://www.rubicondev.com/?page_id=116
// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;
float4x4 tWV: WORLDVIEW;
float4x4 tWVIT: WORLDVIEWINVERSETRANSPOSE;

float farPlane <string uiname="Far Plane";> = 100;

struct vs2ps
{
   float4 Pos: POSITION;
   float4 Data: TEXCOORD0;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------
vs2ps DepthVS(
   float4 PosO: POSITION)
{
   //declare output struct
   vs2ps Out = (vs2ps)0;
   //transform position
   Out.Pos = mul(PosO, tWVP);
   //output depth
   Out.Data.w = Out.Pos.z / farPlane;
   return Out;
}

vs2ps NormalVS(
    float4 PosO: POSITION,
    float4 NormalO: NORMAL)
{
    //declare output struct
    vs2ps Out = (vs2ps)0;
    //transform position
    Out.Pos = mul(PosO, tWVP);
    //Data.xyz is our normal in camera space that the PS wants
    Out.Data.xyz = (mul(NormalO, tWVIT));
    //Out.Data.z *= 2;
    return Out;
}

vs2ps NormalAndDepthVS(
    float4 PosO: POSITION,
    float4 NormalO: NORMAL)
{
    //declare output struct
    vs2ps Out = (vs2ps)0;
    //transform position
    Out.Pos = mul(PosO, tWVP);
    //Data.xyz is our normal in camera space that the PS wants
    Out.Data.xyz = mul(NormalO, tWVIT);
    //depth
    Out.Data.w = Out.Pos.z / farPlane;
    return Out;
}


// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------
float4 DepthPS(vs2ps In) : COLOR
{
    float4 data = 0;
    float depth = In.Data.w;
    data.b = floor(depth * 255) / 255;
    data.a = floor((depth - data.b) * 255 * 255) / 255;
    return data;
}

float4 NormalPS(vs2ps In): COLOR
{
    return float4(In.Data.xyz, 1);
}

float4 NormalAndDepthPS(vs2ps In): COLOR
{
    float4 data;
    // Depth comes from the VS and gets normalised by camera->far
    float depth = In.Data.w;
    // Store the x&y components of the normal in RG
    data.rg = normalize(In.Data.xyz).xy * 0.5 + 0.5;
    // Encode the linear depth across two channels in BA
    data.b = floor(depth * 255) / 255;
    data.a = floor((depth - data.b) * 255 * 255) / 255;
    //data.a = 1;
    return data;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TDepth
{
    pass P0
    {
        AlphaBlendEnable = false;
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_2_0 DepthVS();
        PixelShader  = compile ps_2_0 DepthPS();
    }
}

technique TNormal
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_2_0 NormalVS();
        PixelShader  = compile ps_2_0 NormalPS();
    }
}

technique TNormalAndDepth
{
    pass P0
    {
        AlphaBlendEnable = false;
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_2_0 NormalAndDepthVS();
        PixelShader  = compile ps_2_0 NormalAndDepthPS();
    }
}
