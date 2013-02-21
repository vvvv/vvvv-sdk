
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (DX9)
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (DX9)
float4x4 tWVP: WORLDVIEWPROJECTION;


//texture
texture Tex <string uiname="volumeTexture";>;
float4x4 tTex <string uiname="Texture Transform";>;                  //Texture Transform
sampler3D Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //set the sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = wrap ;
    AddressV = wrap ;
};

float4x4 tTex1 <string uiname="Color Texture Transform";>;                  //Texture Transform

 float Contrast<string uiname="Noise Contrast";> ;

 float4 colorout : COLOR <string uiname="Color";>;

// -------------------------------------------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// -------------------------------------------------------------------------------------------------------------------------------------

struct VS_OUTPUT
{
    float4 Pos  : POSITION;
    float4 TexC : TEXCOORD0;
    float4 TexC1 : TEXCOORD1;
};

VS_OUTPUT VS(
    float4 Pos  : POSITION,
    float4 TexC : TEXCOORD,
    float4 TexC1 : TEXCOORD1)
{
    //inititalize all fields of output struct with 0
    VS_OUTPUT Out = (VS_OUTPUT)0;
    
     Out.TexC1 = mul(TexC,tTex1);
    //transform position
    Pos = mul(Pos, tWVP);
    //transform texturecoordinates

    TexC = mul(TexC, tTex);


    Out.Pos  = Pos;
    Out.TexC =TexC;

    return Out;
}

// -------------------------------------------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// -------------------------------------------------------------------------------------------------------------------------------------

float4 PS(float4 TexC: TEXCOORD0,float4 TexC1: TEXCOORD1): COLOR
{
    float4 col = tex3D(Samp, TexC);  // volumetexture
     col -= 0.5;         //
    col *= Contrast;	// aply contrast
    col += 0.5;
    col.a = col ;      //
    float4 col2 = colorout;

    return  col * col2;
}

// -------------------------------------------------------------------------------------------------------------------------------------
// TECHNIQUES:
// -------------------------------------------------------------------------------------------------------------------------------------

technique TSimpleShader
{
    pass P0
    {
        VertexShader = compile vs_1_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}

technique TFallbackGouraudDirectionalFF
{
    pass P0
    {
        //transformations
        WorldTransform[0]   = (tW);
        ViewTransform       = (tV);
        ProjectionTransform = (tP);

        //material
        MaterialAmbient  = {1,1,1,1};
        MaterialDiffuse  = {1,1,1,1};


        //texturing
        Sampler[0] = (Samp);
        TextureTransform[0] = (tTex);
        TextureTransformFlags[0] = COUNT3;

        //lighting
        LightEnable[0] = TRUE;
        Lighting       = TRUE;

        LightType[0]     = DIRECTIONAL;
        LightAmbient[0]  = (colorout);

        LightDirection[0] = (1,1,1,1);

        //shading
        ShadeMode = GOURAUD;
        VertexShader = NULL;
        PixelShader  = NULL;
    }
}
