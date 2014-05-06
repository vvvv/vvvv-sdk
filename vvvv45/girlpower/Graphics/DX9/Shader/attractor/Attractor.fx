//
// example patch: girlpower\AttractorVS.v4p
//

// -------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS:
// -------------------------------------------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (DX9)
float4x4 tWV: WORLDVIEW;
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (DX9)

float4x4 tCocoon;

float4 C: COLOR;
float4 X;
float4 Y;
float Radius;
float InnerRadius;
float Power;
float Strength;

// -------------------------------------------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// -------------------------------------------------------------------------------------------------------------------------------------

//data that ist returned by the vertexshader
struct VS_OUTPUT
{
    float4 Pos: POSITION;
    float4 Col: COLOR;
};

VS_OUTPUT VS(
    float4 tPos: POSITION,
    float2 cTex: TEXCOORD0)
{
    VS_OUTPUT Out = (VS_OUTPUT)0;

    tPos = mul(tPos, tW);
    float4 orig = mul(float4(0, 0, 0, 1), tW);
    float2 d, at, dsum;
    float s, m;
    dsum = 0;
    int i;
    for (i = 0; i < 4; i++)
    {
      at = float2(X[i], Y[i]);
      // distance from attractor
      d = orig - at;
      float2 normd = normalize(d);
      if (length(d)<Radius)
      {
        // value 0..1 center of attractorcircle .. edge of attractorcircle
        s = length(d)/Radius;
        
        // morph depending on attractor strength
        m = Strength * (pow(s, Power) * sign(s) / s - 1);

        // accumulate shifted point
        dsum += d*m;
      }
    }
    // transform position
    tPos.xy = tPos + dsum;
    tPos = mul(tPos, tV);
    Out.Pos = mul(tPos, tP);
    Out.Col = C;

    return Out;
}

// -------------------------------------------------------------------------------------------------------------------------------------
// TECHNIQUES:
// -------------------------------------------------------------------------------------------------------------------------------------

technique TAttractCocoons
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = null;
    }
}
