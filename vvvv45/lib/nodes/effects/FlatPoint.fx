//@author: vvvv group
//@help: Basic vertex based flat lightning with point light
//@tags: shading
//@credits:

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (EX9)

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

//texture
texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

technique TFixedFunction
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
        ShadeMode = FLAT;
        VertexShader = NULL;
        PixelShader  = NULL;
    }
}
