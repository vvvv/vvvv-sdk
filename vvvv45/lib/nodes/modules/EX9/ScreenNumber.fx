// this shader is used by the multiscreen modules

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (EX9)
float4x4 tWVP: WORLDVIEWPROJECTION;

//texture
texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state
{
    Texture   = (Tex);
   // MagFilter = POINT;
};

int ViewIndex: VIEWPORTINDEX;
int ViewCount: VIEWPORTCOUNT;
int ViewCountx = 1;
int ViewCounty = 1;
float4 backcolor : COLOR <String uiname="Back Color";>  = {.1, .1, .1, 0.7};
float4 fontcolor : COLOR <String uiname="Font Color";>  = {.8, .8, .8, 0.8};


struct vs2ps
{
    float4 Pos : POSITION;
    float2 TexCd : TEXCOORD0;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------

vs2ps VS(
    float4 Pos : POSITION )
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    //transform position
    Out.Pos = mul(Pos, tWVP);
    
    Out.TexCd.x = 0.5 + Pos.x;
    Out.TexCd.y = 0.5 - Pos.y;

    Out.TexCd.x = (Out.TexCd.x + ((ViewIndex+0.01) % ViewCountx)) / ViewCountx ;
    Out.TexCd.y = (Out.TexCd.y + ceil(ViewIndex / ViewCountx)) / ViewCounty ;

    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 PS(vs2ps In): COLOR
{
    float4 col = tex2D(Samp, In.TexCd);
    return float4(fontcolor * col.rgb, col.r * fontcolor.a) + backcolor;
    //return (viewindex > float(1));
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TSimpleShader
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
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

        //texturing
        Sampler[0] = (Samp);
        TexCoordIndex[0] = 0;
        TextureTransformFlags[0] = COUNT4 | PROJECTED;
        //Wrap0 = U;  // useful when mesh is round like a sphere
        
        Lighting       = FALSE;

        //shaders
        VertexShader = NULL;
        PixelShader  = NULL;
    }
}
