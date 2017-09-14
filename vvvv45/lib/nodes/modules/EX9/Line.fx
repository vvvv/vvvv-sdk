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
float4x4 tPInv: PROJECTIONINVERSE;   //projection matrix as set via Renderer (EX9)
float4x4 tWV: WORLDVIEW;
float4x4 tWVP: WORLDVIEWPROJECTION;
 
//material properties
float4 cAmb : COLOR <String uiname="Color";>  = {1, 1, 1, 1};

//texture
texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = Anisotropic;         //sampler states
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Wrap;
};

float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

float3 Point1 = {-0.5, 0, 0};
float3 Point2 = { 0.5, 0, 0};

float Width = 0.01;


//the data structure: "vertexshader to pixelshader"
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
    float4 Pos : POSITION;
    float4 TexCd : TEXCOORD0;
    float2 uv: TEXCOORD2;
};


    //float3 curve=point1+((point2-point1)*range)+(range-pow(range,2))*gamma;
    //point1*(1-range) + point2*range + range*(1-range)*gamma

float3 slerp(float3 A, float3 B, float S)
{
   float omega = acos( dot(A, B) / max(0.00001, (length(A) * length(B))) );

   return (A * sin((1-S) * omega) + B * sin(S * omega)) / sin(omega);
}

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------

vs2ps VS_ConstantWidth(
    float4 Pos : POSITION,
    float4 TexCd : TEXCOORD0,
    float depth : TEXCOORD1)
{
    float w = Width * 0.003;

    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;
    
    float u=Pos.x+0.5;
    Out.uv = float2(u, Pos.y*2);
    
    // get point on curve
    float4 p;
    //p = float4(lerp(Point1, Point2, u), 1);

    // get position in projection space
    //p = mul(p, tWVP);

    // get tangent in projection space
    float4 p1 = mul(float4(Point1, 1), tWVP);
    float4 p2 = mul(float4(Point2, 1), tWVP);

    p = lerp(p1, p2, u);

    p1 /= p1.w;
    p2 /= p2.w;
    float4 tangent = p2 - p1;

    //p = lerp(p1, p2, u);

    // get normal in projection space
    float2 normal = normalize(float2(tangent.y, -tangent.x));

    // translate point to get a thick curve
    float2 off = Pos.y * normal * w * p.w;

    // correct aspect ratio
    off *= mul(float4(1, 1, 0, 0), tP);

    p+= float4(off, 0, 0);

    //tangent = normalize(tangent);
    //float3 normal = cross(tangent, float3(0,0,1));
    //p += Pos.y * float4(normal, 0) * w * p.w;

    // output pos p
    Out.Pos = p;

    TexCd.x *= .1 * length(tangent) / w;

    //ouput texturecoordinates
    Out.TexCd = mul(TexCd, tTex);

    return Out;
}


// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 PS(vs2ps In): COLOR
{
    float4 col = tex2D(Samp, In.TexCd) * cAmb;
    //col.a *= 1 - pow(abs(In.uv.y), 4);
    return col;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TLine
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_2_0 VS_ConstantWidth();
        PixelShader = compile ps_2_0 PS();
    }
}


