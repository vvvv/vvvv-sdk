//@author: vvvv group
//@help: Creates a dot in the center of the texture coordinates with adjustable edge fading
//@tags: dot
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
texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

//texture
texture HaloTex <string uiname="Halo Texture";>;
sampler HaloSamp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (HaloTex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

//texture transformation marked with semantic TEXTUREMATRIX to achieve symmetric transformations
float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;
float4x4 tHalo: TEXTUREMATRIX <string uiname="Halo Transform";>;

float Alpha = 1;
float rInner <String uiname="Inner Radius"; float uimin=0;> = 0.3;
float rOuter <String uiname="Outer Radius"; float uimin=0;> = 0.45;
float4 cInner  : COLOR <String uiname="Inner Color";>  = {1, 1, 1, 1};
float4 cOuter  : COLOR <String uiname="Outer Color";>  = {0, 0, 0, 0};

//the data structure: "vertexshader to pixelshader"
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
    float4 Pos  : POSITION;
    float2 TexCd : TEXCOORD0;
    float2 TexCdHalo : TEXCOORD1;
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
    Out.TexCdHalo = mul(TexCd, tHalo);

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

float4 PSCosineHalo(vs2ps In): COLOR
{
    float thickness = rOuter - rInner;
    float halo = clamp(distance(float2(0, 0), In.TexCdHalo-0.5)-rInner, 0, thickness);
    float4 col = CosineInterpolate(cInner, cOuter, halo/thickness);
    
    col *= tex2D(Samp, In.TexCd);
    col.a *= Alpha;
    return col;
}

float4 PSLinearHalo(vs2ps In): COLOR
{
    float thickness = rOuter - rInner;
    float halo = clamp(distance(float2(0, 0), In.TexCdHalo-0.5)-rInner, 0, thickness);
    float4 col = lerp(cInner, cOuter, halo/thickness);

    col *= tex2D(Samp, In.TexCd);
    col.a *= Alpha;
    return col;
}

float4 PSTextureHalo(vs2ps In): COLOR
{
    float halo = distance(float2(0, 0), In.TexCdHalo-0.5)/sqrt(0.5);
    float4 col = tex2D(HaloSamp, float2(halo, 0));

    col *= tex2D(Samp, In.TexCd) * cInner;
    col.a *= Alpha;
    return col;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TLinearHalo
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PSLinearHalo();
    }
}

technique TCosineHalo
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PSCosineHalo();
    }
}

technique TTextureHalo
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_2_0 PSTextureHalo();
    }
}
