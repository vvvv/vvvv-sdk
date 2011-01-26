//@author: fibo
//@help: example shader using Attractor3d.fxh
//@tags: attractor, 3d
//@credits: kalle for the help patch
// -------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS:
// -------------------------------------------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (DX9)
float4x4 tWV: WORLDVIEW;
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (DX9)

float4 C: COLOR <String uiname="Color";>  = {1, 1, 1, 1};
float3 Center;
float Radius;
float Power;
float Strength;

#include <effects\Attractor3d.fxh>

// -------------------------------------------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// -------------------------------------------------------------------------------------------------------------------------------------

//data that ist returned by the vertexshader
struct VS_OUTPUT
{
    float4 Pos: POSITION;
    float4 Col: COLOR;
};

VS_OUTPUT VS1 ( float4 tPos: POSITION )
{
    VS_OUTPUT Out = (VS_OUTPUT)0;

    tPos = mul(tPos, tW);
    tPos.xyz=attractor3d(tPos.xyz,Center,Power,Strength,Radius);
    tPos = mul(tPos, tV);
          
    Out.Pos = mul(tPos, tP);
    Out.Col = C;

    return Out;
}

// -------------------------------------------------------------------------------------------------------------------------------------
// TECHNIQUES:
// -------------------------------------------------------------------------------------------------------------------------------------

technique attractor3D
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS1();
        PixelShader = null;
    }
}
