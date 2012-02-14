//@author: vvvv group
//@help: draws a mesh with environment cube map. like a reflection
//@tags: reflection, metal, mirror
//@credits:

// -----------------------------------------------------------------------------
// PARAMETERS:
// -----------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (DX9)
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (DX9)
float4x4 tWVP: WORLDVIEWPROJECTION;

texture texCubeMap <string uiname="Texture";>;

sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (texCubeMap);   //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

float4x4 tTex : CUBETEXTUREMATRIX <string uiname="Texture Transform";>;

technique TBoundingBoxFF
{
  pass P0
  {
    WorldTransform[0] = <tW>;
    ViewTransform = <tV>;
    ProjectionTransform = <tP>;

    // Pixel state
    Texture[0] = <texCubeMap>;

    MinFilter[0] = Linear;
    MagFilter[0] = Linear;

    AddressU[0] = Clamp;
    AddressV[0] = Clamp;
    AddressW[0] = Clamp;

    ColorOp[0] = SelectArg1;
    ColorArg1[0] = Texture;

    TextureTransform[0] = (tTex);
    TexCoordIndex[0] = CAMERASPACEPOSITION;
    TextureTransformFlags[0] = Count3;

    //shader
    VertexShader = null;
    PixelShader = null;

  }
}


technique TChromeFF
{
  pass P0
  {
    WorldTransform[0] = <tW>;
    ViewTransform = <tV>;
    ProjectionTransform = <tP>;

    // Pixel state
    Texture[0] = <texCubeMap>;

    MinFilter[0] = Linear;
    MagFilter[0] = Linear;

    AddressU[0] = Clamp;
    AddressV[0] = Clamp;
    AddressW[0] = Clamp;

    ColorOp[0] = SelectArg1;
    ColorArg1[0] = Texture;

    TextureTransform[0] = (tTex);
    TexCoordIndex[0] = CameraSpaceReflectionVector;
    TextureTransformFlags[0] = Count3;

    //shader
    VertexShader = null;
    PixelShader = null;

  }
}
