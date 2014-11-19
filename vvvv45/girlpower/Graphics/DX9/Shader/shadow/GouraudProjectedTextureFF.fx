// Gouraud-Lighting : calculate color (lighted) for the vertices and interpolate over the pixels

// -------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS:
// -------------------------------------------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (DX9)
float4x4 tWV: WORLDVIEW;
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (DX9)

//material properties
float4 mAmb  : COLOR <String uiname="Ambient Color";>  = {  0.6,    0.6,    0.6,    1.00000  };
float4 mDiff : COLOR <String uiname="Diffuse Color";>  = {  0.7,    0.7,    0.7,    1.00000  };
float4 mSpec : COLOR <String uiname="Specular Color";> = {  0.63,   0.63,   0.63,   1.00000  };
float mPower <String uiname="Power"; float uimin=0.0;> = 10.0;   //shininess of specular highlight

//light properties
float4 lAmb  : COLOR <String uiname="Ambient Light";>  = {  0.4,    0.4,   0.4,     1.00000  };
float4 lDiff : COLOR <String uiname="Diffuse Light";>  = {  0.63,   0.63,  0.63,    1.00000  };
float4 lSpec : COLOR <String uiname="Specular Light";> = {  0.46,   0.46,  0.46,    1.00000  };
float3 lDir <string uiname="Light Direction";>  = {   0.577,   -0.577,   0.577  };          //Light Direction ( in view space !! )

//texture
texture Tex <string uiname="Texture";>;
float4x4 tTex <string uiname="Texture Transform";>;                  //Texture Transform
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //set the sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};


technique TFixedFunction
{
    pass P0
    {
        //transforms
        WorldTransform[0]   = (tW);
        ViewTransform       = (tV);
        ProjectionTransform = (tP);

        //material
        MaterialAmbient  = (mAmb);
        MaterialDiffuse  = (mDiff);
        MaterialSpecular = (mSpec);
        MaterialPower    = (mPower);

        //texturing
        Sampler[0] = (Samp);
        TextureTransform[0] = (tTex);
        TexCoordIndex[0] = CAMERASPACEPOSITION;
        TextureTransformFlags[0] = COUNT4 | PROJECTED;
        
        //lighting
        LightType[0]      = DIRECTIONAL;
        LightAmbient[0]   = (lAmb);
        LightDiffuse[0]   = (lDiff);
        LightSpecular[0]  = (lSpec);
        LightDirection[0] = (lDir);

        LightEnable[0] = TRUE;
        Lighting       = TRUE;
        SpecularEnable = TRUE;

        ShadeMode = GOURAUD;

        //shaders
        VertexShader = NULL;
        PixelShader  = NULL;
    }
}
