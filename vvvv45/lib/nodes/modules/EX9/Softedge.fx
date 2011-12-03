// this shader draws an alpha gradient

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (EX9)
float4x4 tWVP: WORLDVIEWPROJECTION;


int ViewIndex: VIEWPORTINDEX;
int ViewCount: VIEWPORTCOUNT;
int ViewCountx = 1;
int ViewCounty = 1;
float Gamma;
int LeftTopRightBottom;


//the data structure: "vertexshader to pixelshader"
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
    float4 Pos : POSITION;
    float1 TexCd : TEXCOORD0;
    bool dosoft: TEXCOORD1;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------

vs2ps VS(
    float4 Pos : POSITION )
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    //transform position
    Out.Pos = mul(Pos, tWVP);
    
    Out.TexCd.x = 0.5 + Pos.x;

    int viewx = (ViewIndex+0.01) % ViewCountx;
    int viewy = ceil(ViewIndex / ViewCountx);

    Out.dosoft = (((viewx > 0) && (LeftTopRightBottom==0)) ||
        ((viewy > 0) && (LeftTopRightBottom==1)) ||
        ((viewx < ViewCountx-1) && (LeftTopRightBottom==2)) ||
        ((viewy < ViewCounty-1) && (LeftTopRightBottom==3)));
        
    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 PS(vs2ps In): COLOR
{

    if (In.dosoft.x==1)
    {
    float4 col = float4(0, 0, 0, 1);
      col.a = clamp(In.TexCd.x, 0, 1);
      col.a = pow(col.a, Gamma);
      col.a = 1 - col.a;
      return col;
    }
    else
    return 0;

}


// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TSoftEdge
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
