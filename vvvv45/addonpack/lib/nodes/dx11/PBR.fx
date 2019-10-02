//@author: flux
//@help: Physically based BRDF analytic light
//@tags: GGX, Lambert, Disney, Phong, Gouraud

#define PI_2 6.28318531
#define PI    3.14159265

cbuffer cbPerDraw : register(b0)
{
	float4x4 tLAV: LAYERVIEW;
	float4x4 tP: PROJECTION;
	float4x4 tLVP: LAYERVIEWPROJECTION;
};

struct LightBuffer
{
	float3 pos;
	float lum;
	float3 dir;
	float rad;
	float3 col;	
	float ang;
	float type;
};

StructuredBuffer<LightBuffer> light : GGX_LIGHTS;

struct SurfaceProp
{
	float3x3 tbn;
	float4 mat;    //metallic, roughness, 0, 0
	float3 vDirV;
	bool iso;
	float3 albedo;
	bool disney;	
};

cbuffer cbPerObj : register(b1)
{
	float4x4 tW : WORLD;
	float4x4 tWV: WORLDVIEW;
	float4x4 tWIT: WORLDINVERSETRANSPOSE;
	float Alpha <float uimin=0.0; float uimax=1.0;> = 1;
	bool disney <String uiname="DiffuseBRDF";> = 1;
	float4 col <bool color=true;String uiname="Albedo Color";> = { 1.0f,1.0f,1.0f,1.0f };
	float metal <float uimin=0.0; float uimax=1.0;String uiname="Metalness";> = 0;
	float rough <float uimin=0.0; float uimax=1.0;String uiname="Roughness";> = 1;
};

cbuffer cbLayerSemantics : register(b2)
{
	int tonemap : GAMMA_CORRECT;
}

struct vsInput
{
    float4 PosO : POSITION;
	float3 NormO : NORMAL;
	float4 uv: TEXCOORD0;
};

struct psInput
{
    float4 posScreen : SV_Position;
    float3 NormV: NORMAL;   
    float3 vDirV: TEXCOORD0;
};

#include "BRDF.fxh"


//______________________________________________________________________________

psInput VS(vsInput In)
{
    psInput Out;
  
	Out.NormV = normalize(mul(mul(In.NormO, (float3x3)tWIT),(float3x3)tLAV));		
    Out.posScreen  = mul(In.PosO, mul(tW,tLVP));
	Out.vDirV = mul(float4(In.PosO.xyz,1),tWV).xyz;
	
	return Out;
}
//______________________________________________________________________________


float4 PS(psInput In): SV_Target
{	
	// to linear color space
	float3 albedo = pow(abs(col.rgb),2.2f);
	float4 mat = float4(metal,rough, -.2, -.2);

	float3x3 tbn;
	tbn[2] = normalize(In.NormV);
		
	SurfaceProp p = {tbn, mat, In.vDirV, -.2, albedo, disney};
	float3 result = 0;
	
	uint lightSize, dummy;
    light.GetDimensions(lightSize, dummy);

	for (uint i = 0; i < lightSize; i++) 
	{
		if(light[i].type >= 0)
			result += GGX(p, light[i]);	
	}	

	result = tonemap ? ToneMapper(result) : result;
    return float4(result,Alpha);
}
//______________________________________________________________________________


technique11 Isotropic
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetPixelShader( CompileShader( ps_5_0, PS() ) );
	}
}
