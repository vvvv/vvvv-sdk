//@author: vvvv group
//@help: Basic vertex based lightning with point light
//@tags: shading, blinn
//@credits:

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (DX9)
float4x4 tWV: WORLDVIEW;
float4x4 tWVP: WORLDVIEWPROJECTION;
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (DX9)

//light properties
float3 lPos <string uiname="Light Position";> = {0, 5, -2};       //light position in world space
float lAtt0 <String uiname="Light Attenuation 0"; float uimin=0.0;> = 0;
float lAtt1 <String uiname="Light Attenuation 1"; float uimin=0.0;> = 0.3;
float lAtt2 <String uiname="Light Attenuation 2"; float uimin=0.0;> = 0;
float4 lAmb  : COLOR <String uiname="Ambient Color";>  = {0.15, 0.15, 0.15, 1};
float4 lDiff : COLOR <String uiname="Diffuse Color";>  = {0.85, 0.85, 0.85, 1};
float4 lSpec : COLOR <String uiname="Specular Color";> = {0.35, 0.35, 0.35, 1};
float lPower <String uiname="Power"; float uimin=0.0;> = 25.0;     //shininess of specular highlight
float lRange <String uiname="Light Range"; float uimin=0.0;> = 10.0;

float Alpha <float uimin=0.0; float uimax=1.0;> = 1;

//texture
texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //set the sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;
float4x4 tColor <string uiname="Color Transform";>;

struct vs2ps
{
    float4 PosWVP: POSITION;
    float4 TexCd : TEXCOORD0;
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------

vs2ps VS(
    float4 PosO: POSITION,
    float3 NormO: NORMAL,
    float4 TexCd : TEXCOORD0)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    //distance from light to vertex
    float4 PosW = mul(PosO, tW);
    float d = distance(PosW, lPos);

    float atten = 0;

    //compute attenuation only if vertex within lightrange
    if (d<lRange)
    {
       atten = 1/(saturate(lAtt0) + saturate(lAtt1) * d + saturate(lAtt2) * pow(d, 2));
    }
    float4 amb = atten * lAmb;
    amb.a = 1;
    
    //inverse light direction in view space
    float3 LightDirW = normalize(lPos - PosW);
    float3 LightDirV = mul(LightDirW, tV);
    
    //normal in view space
    float3 NormV = normalize(mul(NormO, tWV));

    //view direction = inverse vertexposition in viewspace
    float3 PosV = mul(PosW, tV);
    float3 ViewDirV = normalize(-PosV);
    //halfvector
    float3 H = normalize(ViewDirV + LightDirV);

    //compute blinn lighting
    float4 shades = lit(dot(NormV, LightDirV), dot(NormV, H), lPower);

    float4 diff = lDiff * shades.y * atten;
    diff.a = 1;
    float4 spec = lSpec * shades.z * atten;
    spec.a = 1;

    //position (projected)
    Out.PosWVP  = mul(PosO, tWVP);
    Out.TexCd = mul(TexCd, tTex);
    Out.Diffuse = amb + diff;
    Out.Specular = spec;

    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 PS(vs2ps In): COLOR
{
    //In.TexCd = In.TexCd / In.TexCd.w; // for perpective texture projections (e.g. shadow maps) ps_2_0

    float4 col = tex2D(Samp, In.TexCd);
    col.rgb *= In.Diffuse + In.Specular;
    col = mul(col, tColor);
    col.a *= Alpha;

    return col;
}


// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TGouraudPoint
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_1_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}

technique TGouraudPointFF
{
    pass P0
    {
        //transformations
        NormalizeNormals = true;
        WorldTransform[0]   = (tW);
        ViewTransform       = (tV);
        ProjectionTransform = (tP);

        //material
        MaterialAmbient  = {1, 1, 1, 1};
        MaterialDiffuse  = {1, 1, 1, 1};
        MaterialSpecular = {1, 1, 1, 1};
        MaterialPower    = (lPower);

        //texturing
        Sampler[0] = (Samp);
        TextureTransform[0] = (tTex);
        TexCoordIndex[0] = 0;
        TextureTransformFlags[0] = COUNT2;
        //Wrap0 = U;  // useful when mesh is round like a sphere

        //lighting
        LightEnable[0] = TRUE;
        Lighting       = TRUE;
        SpecularEnable = TRUE;
        
        LightType[0]     = POINT;
        LightAmbient[0]  = (lAmb);
        LightDiffuse[0]  = (lDiff);
        LightSpecular[0] = (lSpec);
        LightPosition[0] = (lPos);
        LightRange[0]    = (lRange);
        LightAttenuation0[0] = (lAtt0);
        LightAttenuation1[0] = (lAtt1);
        LightAttenuation2[0] = (lAtt2);

        //shading
        ShadeMode = GOURAUD;
        VertexShader = NULL;
        PixelShader  = NULL;
    }
}
