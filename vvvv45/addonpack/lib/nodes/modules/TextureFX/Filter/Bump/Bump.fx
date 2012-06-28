//@author: vvvv group
//@help: basic pixel based lightning with directional light
//@tags: shading, blinn
//@credits:

// -----------------------------------------------------------------------------
// PARAMETERS:
// -----------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tWV: WORLDVIEW;
float4x4 tWVP: WORLDVIEWPROJECTION;
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (EX9)

#include "Bump.fxh"

texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};

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
float4x4 tColor <string uiname="Color Transform";>;
float BumpAmount <float uimin=0.0;> =1.0;
float Brightness <float uimin=0.0;> =1.0;

struct vs2ps
{
    float4 PosWVP: POSITION;
    float4 TexCd : TEXCOORD0;
    float3 LightDirV: TEXCOORD1;
    float3 NormV: TEXCOORD2;
    float3 ViewDirV: TEXCOORD3;
	float3 PosW: TEXCOORD4;
};

// -----------------------------------------------------------------------------
// VERTEXSHADERS
// -----------------------------------------------------------------------------
float2 R;
vs2ps VSdir(
    float4 PosO: POSITION,
    float3 NormO: NORMAL,
    float4 TexCd : TEXCOORD0)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;
	PosO.xy*=2;
    //inverse light direction in view space
    Out.LightDirV = normalize(mul(lDir, tV));
    
    //normal in view space
    Out.NormV = normalize(mul(NormO, tWV));

    //position (projected)
    Out.PosWVP  = mul(PosO, tWVP);
    Out.TexCd = mul(TexCd, tTex);
    Out.ViewDirV = -normalize(mul(float4(PosO.xy*.0,-1,PosO.w), tWV));
    return Out;
}
vs2ps VSpnt(
    float4 PosO: POSITION,
    float3 NormO: NORMAL,
    float4 TexCd : TEXCOORD0)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;
	PosO.xy*=2;
    Out.PosW = mul(PosO, tW);
	Out.PosW.xy*=R/R.x;
    //inverse light direction in view space
    float3 LightDirW = normalize(lPos - Out.PosW);
    Out.LightDirV = -mul(LightDirW, tV);
    
    //normal in view space
    Out.NormV = normalize(mul(NormO, tWV));

    //position (projected)
    Out.PosWVP  = mul(PosO, tWVP);
    Out.TexCd = mul(TexCd, tTex);
	
    Out.ViewDirV = -normalize(mul(float4(PosO.xy*0,-1,PosO.w), tWV));
    return Out;
}
// -----------------------------------------------------------------------------
// PIXELSHADERS:
// -----------------------------------------------------------------------------

float Alpha <float uimin=0.0; float uimax=1.0;> = 1;

float4 PSdir(vs2ps In): COLOR
{
    //In.TexCd = In.TexCd / In.TexCd.w; // for perpective texture projections (e.g. shadow maps) ps_2_0

    float4 col = tex2D(Samp, In.TexCd);
	float3 nor=tex2D(s0, In.TexCd);
	nor=normalize(float3(nor.xy-.5,nor.z));
	In.NormV.xyz=lerp(In.NormV.xyz,nor,BumpAmount);
    col.rgb *= PhongDirectional(In.NormV, In.ViewDirV, In.LightDirV);
	col.rgb*=Brightness;
    col.a *= Alpha;
    return mul(col, tColor);
}
float4 PSpnt(vs2ps In): COLOR
{
    //In.TexCd = In.TexCd / In.TexCd.w; // for perpective texture projections (e.g. shadow maps) ps_2_0
    
    float4 col = tex2D(Samp, In.TexCd);
	float3 nor=tex2D(s0, In.TexCd);
	nor=normalize(float3(nor.xy-.5,nor.z));
	In.NormV.xyz=lerp(In.NormV.xyz,nor,BumpAmount);
    col.rgb *= PhongPoint(In.PosW, In.NormV, In.ViewDirV, In.LightDirV);
    col.rgb*=Brightness;
	col.a *= Alpha;
    return mul(col, tColor);
}

// -----------------------------------------------------------------------------
// TECHNIQUES:
// -----------------------------------------------------------------------------

technique TPhongDirectional
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        vertexshader=compile vs_3_0 VSdir();
        pixelshader=compile ps_3_0 PSdir();
    }
}
technique TPhongPoint
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        vertexshader=compile vs_3_0 VSpnt();
        pixelshader=compile ps_3_0 PSpnt();
    }
}