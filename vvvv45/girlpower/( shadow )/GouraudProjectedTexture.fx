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

float4x4 tColor <string uiname="Color Transform";>;


// -------------------------------------------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// -------------------------------------------------------------------------------------------------------------------------------------

//data that ist returned by the vertexshader
struct VS_OUTPUT
{
    float4 Pos  : POSITION;
    float4 Diff : COLOR0;
    float4 Spec : COLOR1;
    float4 Amb  : TEXCOORD1;
    float4 TexC : TEXCOORD0;
};

VS_OUTPUT VS(
    float4 Pos  : POSITION,
    float3 Norm : NORMAL)
{
    //inititalize all fields of output struct with 0
    VS_OUTPUT Out = (VS_OUTPUT)0;
    //diffuse Light direction
    float3 L = -lDir;


    // transform position to texture coordinate (this is the tricky line)
    Out.TexC = mul( Pos, tTex );

    // transform position
    Pos = mul(Pos, tWV);

    Norm = normalize(mul(Norm, tWV));
    //normal (view space)
    float3 N = Norm.xyz;

    //reflection vector (view space)
    float3 R = normalize(2 * dot(N, L) * N - L);
    //view direction (view space)
    float3 V = -normalize(Pos.xyz);

    L = normalize(L);
    //calculate diffuse light
    float4 diff = max(dot(N, L), 0) * mDiff * lDiff;
    diff.a = 1.0;
    
    float4 spec = pow(max(dot(R, V),0), mPower) * mSpec * lSpec;
    
    Out.Pos  = mul(Pos, tP);
    Out.Diff = diff;
    Out.Spec = spec;
    Out.Amb  = lAmb * mAmb;

    return Out;    
}

// -------------------------------------------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// -------------------------------------------------------------------------------------------------------------------------------------

float4 PSGouraud(
    float4 Diff : COLOR0,
    float4 Spec : COLOR1,
    float4 Amb  : TEXCOORD1,
    float4 TexC : TEXCOORD0 ): COLOR
{
    float4 col = tex2Dproj(Samp, TexC);
    col = mul(col, tColor);
    return col * (Diff + Spec + Amb);
}

float4 PSDiffuse(
    float4 Diff : COLOR0,
    float4 TexC : TEXCOORD0 ): COLOR
{
    float4 col = tex2Dproj(Samp, TexC);
    col = mul(col, tColor);
    return col * Diff;
}

float4 PSSpecular(
    float4 Spec : COLOR1,
    float4 TexC : TEXCOORD0 ): COLOR
{
    float4 col = tex2Dproj(Samp, TexC);
    col = mul(col, tColor);
    return col * Spec;
}

float4 PSAmbient(
    float4 Amb : TEXCOORD1,
    float4 TexC : TEXCOORD0 ): COLOR
{
    float4 col = tex2Dproj(Samp, TexC);
    col = mul(col, tColor);
    return col * Amb;
}

// -------------------------------------------------------------------------------------------------------------------------------------
// TECHNIQUES:
// -------------------------------------------------------------------------------------------------------------------------------------

technique TGouraud
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_2_0 PSGouraud();
    }
}


technique TDiffuseOnly
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_2_0 PSDiffuse();
    }
}

technique TSpecularOnly
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_2_0 PSSpecular();
    }
}

technique TAmbientOnly
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_2_0 PSAmbient();
    }
}


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
        TexCoordIndex[0] = 0;
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
