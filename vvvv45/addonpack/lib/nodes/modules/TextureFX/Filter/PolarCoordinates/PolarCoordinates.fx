
//@author: mtallen, dep, Libero Spagnolini
//@help: coordinate system conversion
//@tags: polar, cartesian, coordinate system, twirl, squeeze, zoom, lens
//@credits: mtallen, dep, Libero Spagnolini
// Polar Coordinates
//

// -----------------------------------------------------------------------------
// PARAMETERS:
// -----------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;

float interpolation <string uiname="interpolation";> = 1.0;
//texture
texture Image <string uiname="Input Image";>;
//int width = Image.width;
//int height = Image.height;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Image);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

// texture transformation marked with semantic TEXTUREMATRIX to achieve
// symmetric transformations
float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

// slider for input
float slider<string uiname="Repeats";> = 1;
bool repeat<string uiname="Clamp";> = 1;
float scale<string uiname="Scale";> = 1;
float twirl<string uiname="Twirl";> = 0;
float squeeze<string uiname="Squeeze";> = 1;
float zoom<string uiname="Zoom";> = 1;
float lens<string uiname="Lens";> = 0;
float offsetx<string uiname="x offset";> = 0;
float offsety<string uiname="y offset";> = 0;

// Helper functions
float2 cartesian(float2 coords)
{
    coords[0] -= (0.5 + offsetx);
    coords[1] -= (0.5 + offsety);
    return coords;
}
float2 polar(float2 coords)
{
    coords = coords / 2.0;
    coords += 0.5;
    return coords;
}

float2 cartToPolar(float2 coords)
{
    float mag = (length(coords)/0.5)*scale;
    // clamp it
    if (!(repeat))
    {
        mag = saturate(mag);
    }
    // angle = arc tangent of y/x
    float angle = atan2(coords[1], coords[0])*slider;
    angle = -(angle+1.57079633)/6.28319;
    
    mag = pow(mag, 1.0/squeeze)*zoom;	//squeeze & zoom
    mag = lerp(mag, mag*mag/sqrt(2.0), lens);	//fisheye/lens effect
    angle += (1.0 - smoothstep(-1.0, 1.0, mag))*twirl;     //twirl
    
    coords[0] = angle;
    coords[1] = mag;
    return coords;
}
float2 polarToCart(float2 coords)
{
    float mag = coords[1];
    float angle = -1.0*coords[0]*6.28319+1.57079633;

    coords[0] = mag*cos(angle);
    coords[1] = mag*sin(angle);
    return coords;
}

//the data structure: "vertexshader to pixelshader"
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
    float4 Pos  : POSITION;
    float2 TexCd : TEXCOORD0;
};

// -----------------------------------------------------------------------------
// Functions
// -----------------------------------------------------------------------------


// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------
vs2ps VS(
    float4 PosO  : POSITION,
    float4 TexCd : TEXCOORD0)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    //transform position
    Out.Pos = mul(PosO, tWVP);

    //transform texturecoordinates
    Out.TexCd = mul(TexCd, tTex);

    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 psPolarCoordinates(vs2ps In): COLOR
{
    /*
     * for the conversion to polar coordinates we will offset the incoming
     * pixels coordinates to be in a cartesian plane with 0.0, 0.0 as the center
     * of the image and the upper left corner being -0.5, 0.5. From here we will
     * use those coordinates as a vector, getting the magnitude and the angle
     * using sweet sweet linear algebra. we clamp the radius to the size of the
     * image (1.0), and we use that and the angle in radians for our new color!
     */
     
    // convert to cartesian
    float2 coords;
    coords = cartesian(In.TexCd);
    // do the algebra to get the angle and magnitude
    //rotate the whole thing 90 CCW (this is what Pshop does)
    coords = cartToPolar(coords);
    coords = lerp(In.TexCd, coords, interpolation);
    float4 col = tex2D(Samp,coords);
    return col;
}

float4 psCartCoordinates(vs2ps In): COLOR
{
    float2 coords = In.TexCd;
    coords[0] = saturate(coords[0])+0.5;
    coords[1] = saturate(coords[1]);
    coords = polarToCart(coords);
    coords = polar(coords);

    coords = lerp(In.TexCd, coords, interpolation) + float2(offsetx, offsety);
    float4 col = tex2D(Samp,coords);
    return col;
}


// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique CartesianToPolar
{
    pass P0
    {
        VertexShader = compile vs_1_0 VS();
        PixelShader  = compile ps_2_0 psPolarCoordinates();
    }
}
technique PolarToCartesian
{
    pass P0
    {
        VertexShader = compile vs_1_0 VS();
        PixelShader  = compile ps_2_0 psCartCoordinates();
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
        TextureTransform[0] = (tTex);
        TexCoordIndex[0] = 0;
        TextureTransformFlags[0] = COUNT2;
        //Wrap0 = U;  // useful when mesh is round like a sphere

        Lighting       = FALSE;

        //shaders
        VertexShader = NULL;
        PixelShader  = NULL;
    }
}
