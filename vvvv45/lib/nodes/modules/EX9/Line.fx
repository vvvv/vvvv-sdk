// Draws a Bezier with 2 Control Points and One Handle in between
// in other words a Bezier with 3 Control Points
// http://www.cs.mtu.edu/~shene/COURSES/cs3621/NOTES/spline/Bezier/bezier-construct.html

//shading:         flat
//lighting model:  constant


// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (EX9)
float4x4 tWVP: WORLDVIEWPROJECTION;

//material properties
float4 cAmb : COLOR <String uiname="Color";>  = {1, 1, 1, 1};

float3 Point1, Point2, Handle;
float Width;

//the data structure: "vertexshader to pixelshader"
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
    float4 Pos : POSITION;
    float4 TexCd : TEXCOORD0;
    float depth : TEXCOORD1;
    float u: TEXCOORD2;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------

vs2ps VS(
    float4 Pos : POSITION,
    float4 TexCd : TEXCOORD0,
    float depth : TEXCOORD1)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    float u=Pos.x+0.5;
    Out.u = u;

    float4 curve = float4(lerp( Point1, Point2, u), 1);

    float delta = 0.002;
    //float pre = u - delta;
    float post = u + delta;
    //float3 curvePre = lerp( Point1, Point2, pre);
    float3 curvePost = lerp( Point1, Point2, post);

    float4 tangent = float4(curvePost - curve, 0);   // - curvePre
    float2 tangent2 = normalize(mul(tangent, tWVP).xy);

    //transform position
    curve = mul(curve, tWVP);

    //make it thick
    curve += Pos.y * float4(tangent2.y, -tangent2.x, 0, 0) * Width * curve.w;

    Out.Pos = curve;

    return Out;
}

vs2ps VS_Bezier_OneHandle(
    float4 Pos : POSITION,
    float4 TexCd : TEXCOORD0,
    float depth : TEXCOORD1)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    float u=Pos.x+0.5;
    Out.u = u;

    //get point on curve with bezier algorithm (for 3 points)
    //float3 curve =                      pow((1-range), 2)  * point1
    //              + 2 *     range *         (1-range)      * handle
    //              +     pow(range, 2)                      * point2;
    float4 curve = float4(lerp( lerp(Point1, Handle, u), lerp(Handle, Point2, u), u), 1);

    float delta = 0.002;
    //float pre = u - delta;
    float post = u + delta;
    //float3 curvePre = lerp( lerp(Point1, Handle, pre), lerp(Handle, Point2, pre), pre);
    float3 curvePost = lerp( lerp(Point1, Handle, post), lerp(Handle, Point2, post), post);

    float4 tangent = float4(curvePost - curve, 0);   // - curvePre
    float2 tangent2 = normalize(mul(tangent, tWVP).xy);

    //transform position
    curve = mul(curve, tWVP);

    //make it thick
    curve += Pos.y * float4(tangent2.y, -tangent2.x, 0, 0) * Width * curve.w;

    Out.Pos = curve;

    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 PS(vs2ps In): COLOR
{
    return cAmb;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TLine_ConstWidth
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}

technique TBezier_OneHandle_ConstWidth
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_2_0 VS_Bezier_OneHandle();
        PixelShader = compile ps_2_0 PS();
    }
}
