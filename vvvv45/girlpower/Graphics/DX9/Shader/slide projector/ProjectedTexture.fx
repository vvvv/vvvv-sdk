// this is an effect template. use it to start writing your own effects.

// -------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS:
// -------------------------------------------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tWV: WORLDVIEW;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (DX9)
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (DX9)
float4x4 tWVP: WORLDVIEWPROJECTION;

float Alpha = 1;
float LodBias = -1;
//texture
texture Tex <string uiname="Texture";>;
float4x4 tTex <string uiname="Texture Transform";>;                  //Texture Transform
float4x4 RCtoTC =
(
 0.5,  0.0,  0.0,  0.0,
 0.0, -0.5,  0.0,  0.0,
 0.0,  0.0,  1.0,  0.0,
 0.5,  0.5,  0.0,  1.0
);
//tTex = mul ( tTex , RCtoTC );
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //set the sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    // BorderColor = { 1.0, .0, 1.0, .0 }; // what to do with border color? alpha does not respond ...
    AddressU = Border;           /////////
    AddressV = Border;           /////////  vorher Clamp, so ein Quatsch
    MipMapLodBias = (LodBias);
};


// -------------------------------------------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// -------------------------------------------------------------------------------------------------------------------------------------

struct VS_OUTPUT
{
    float4 Pos  : POSITION;
    float4 TexC : TEXCOORD0;
};

VS_OUTPUT VS(
    float4 Pos  : POSITION,
    float4 TexC : TEXCOORD)
{
    //inititalize all fields of output struct with 0
    VS_OUTPUT Out = (VS_OUTPUT)0;


    //transform texturecoordinates
    tTex = mul (RCtoTC, tTex   );
    TexC = Pos;
    
    TexC = mul(TexC, tW);
    TexC = mul(TexC, tTex);
    
    Out.TexC = TexC;

    //transform position
    Pos = mul(Pos, tWVP);
    Out.Pos  = Pos;

    return Out;
}

// -------------------------------------------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// -------------------------------------------------------------------------------------------------------------------------------------

float4 PS(float4 TexC: TEXCOORD0): COLOR
{
    float4 col = tex2Dproj(Samp, TexC);

    //if( TexC.w>0.0 )
    if( TexC.w > 0.0 )
    {
    TexC.xyz = TexC.xyz / TexC.w;
    col.a = Alpha;
    }
    else col = 0;
    
    // below: kill all color outside of original texture.
    if( TexC.x < 0 || TexC.y < 0 || TexC.x > 1 || TexC.y > 1 )
    {
    col = 0;
    }

    return col;
}

// -------------------------------------------------------------------------------------------------------------------------------------
// TECHNIQUES:
// -------------------------------------------------------------------------------------------------------------------------------------

technique TSimpleShader
{
    pass P0
    {

       // TextureTransformFlags[0] = COUNT3 | PROJECTED;
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
