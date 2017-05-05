//@author: vvvv group
//@help: Projects a texture from a given view/perspective onto the geometry
//@tags: 
//@credits:

// -------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS:
// -------------------------------------------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tWV: WORLDVIEW;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (DX9)
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (DX9)
float4x4 tWVP: WORLDVIEWPROJECTION;

float Alpha <float uimin=0.0; float uimax=1.0;> = 1;

//texture
texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
float4x4 tTex <string uiname="Texture Transform";>;                  //Texture Transform

float4x4 ProjectorV <string uiname="Projector View";> =
float4x4(
 1.0, 0.0,  0.0,  0.0,
 0.0, 1.0,  0.0,  0.0,
 0.0, 0.0,  1.0,  0.0,
 0.0, 0.0,  0.0,  1.0
); 

float4x4 InverseProjectorV <string uiname="Inverse Projector View";> =
float4x4(
 1.0, 0.0,  0.0,  0.0,
 0.0, 1.0,  0.0,  0.0,
 0.0, 0.0,  1.0,  0.0,
 0.0, 0.0,  0.0,  1.0
);

float4x4 ProjectorP <string uiname="Projector Perspective";> =
float4x4(
 0.5,  0.0,  0.0,  0.0,
 0.0, -0.5,  0.0,  0.0,
 0.0,  0.0,  1.0,  0.0,
 0.5,  0.5,  0.0,  1.0
);

// -------------------------------------------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// -------------------------------------------------------------------------------------------------------------------------------------

struct VS_OUTPUT
{
    float4 Pos  : POSITION;
    float4 TexC : TEXCOORD0;
	float3 normProjectorV : TEXCOORD1;
	float4 angle : TEXCOORD2;
};

VS_OUTPUT VS(
    float4 Pos  : POSITION,
    float4 TexC : TEXCOORD,
	float3 NormO: NORMAL)
{
    //inititalize all fields of output struct with 0
    VS_OUTPUT Out = (VS_OUTPUT)0;
	
	//view matrix of the projector * projection matrix of the projector
	float4x4 ProjectorVP = mul(InverseProjectorV, ProjectorP);
	//RCtoTC needs to be multiplied by this matrix
	//compare TextureSpace (Transform FromProjectionSpace)
	float4x4 RCtoTC =
float4x4(
 0.5,  0.0,  0.0,  0.0,
 0.0, -0.5,  0.0,  0.0,
 0.0,  0.0,  1.0,  0.0,
 0.5,  0.5,  0.0,  1.0
);
	RCtoTC = mul(ProjectorVP, RCtoTC); 
	
    //transform texturecoordinates
    tTex = mul (RCtoTC, tTex   );
    TexC = Pos;
    
    TexC = mul(TexC, tW);
    TexC = mul(TexC, tTex);
    
    Out.TexC = TexC;
	
	float3 NormW = mul(NormO, (float3x3) tW);
	NormW = normalize(NormW);
	
	float3 PosW = mul(Pos, tW);
	
	float4 ProjectorPositionW = float4(0.0, 0.0, 0.0, 1.0);
	ProjectorPositionW = mul(ProjectorPositionW, ProjectorV);
	float3 PosToProjector = ProjectorPositionW - PosW;
	
	Out.angle.x = dot( NormW, normalize(PosToProjector));
	//--------------------------------------------
	
    //transform position
    Pos = mul(Pos, tWVP);
	
	//q&d! this only makes sure that the projected texture is in front of the actual texture (had problems with lessorequal
    Pos.z -= 0.00001;
	Out.Pos  = Pos;
		
    return Out;
}

// -------------------------------------------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// -------------------------------------------------------------------------------------------------------------------------------------

float Far <string uiname="Z CutOff";> = 100;
float4 BorderColor: COLOR <string uiname="Border Color";>;
bool FrontOnly <string uiname="Frontproject Only";> = 1;
float4 PS(float4 TexC: TEXCOORD0, VS_OUTPUT input): COLOR
{
    float4 col = tex2Dproj(Samp, TexC);

    //if( TexC.w>0.0 )
	//if the object is in front of the projector, not behind
    if( TexC.w > 0.0 )
    {
	    TexC.xyz = TexC.xyz / TexC.w;
	    //multiply by given Alpha value
	    col.a *= Alpha;
	    	
	    //show only if projection comes from the front
    	if (FrontOnly)
	    	if(input.angle.x < 0) 
    			col = BorderColor;
	
	    //for more reality (projections are brighter when the angle is steeper)
	    //col.a *= pow( abs(input.normProjectorV.z), 0.5);
    }
    else 
		col = 0;
    
    // below: kill all color outside of original texture.
    if( TexC.x < 0 || TexC.y < 0 || TexC.x > 1 || TexC.y > 1 || TexC.z > Far)
    {
    	col = BorderColor;
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
