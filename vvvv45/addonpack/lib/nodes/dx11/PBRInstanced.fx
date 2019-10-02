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

StructuredBuffer<float4x4> Transform;
StructuredBuffer<float4x4> TransformINV;   //Inverse
StructuredBuffer<float4> AlbedoColor;
StructuredBuffer<float2> MaterialProperties;
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
	float4x4 tWorld : WORLD;
	bool disney <String uiname="DiffuseBRDF";> = 1;
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
	uint ii : SV_InstanceID;
};

struct psInput
{
    float4 posScreen : SV_Position;
    float3 NormV: NORMAL;   
    float3 vDirV: TEXCOORD0;
	float4 col: COLOR;
	float2 matp: TEXCOORD2;
};


#include "BRDF.fxh"

//______________________________________________________________________________

psInput VS(vsInput In)
{
    psInput Out;
  
	float4x4 tW = mul(Transform[In.ii], tWorld);
	float4x4 tWIT = tW;
	
	uint inv, dummy;
    TransformINV.GetDimensions(inv, dummy);
	
	if(inv > 0){
		tWIT = transpose(TransformINV[In.ii]);
	}
	
	float4 PosW  = mul(In.PosO,tW);
	
	Out.NormV = normalize(mul(mul(In.NormO, (float3x3)tWIT),(float3x3)tLAV));		
    Out.posScreen  = mul(PosW,tLVP);
	Out.vDirV = mul(float4(PosW.xyz,1),tLAV).xyz;
	
	uint colSize, matSize;
	AlbedoColor.GetDimensions(colSize, dummy);
	MaterialProperties.GetDimensions(matSize, dummy);
	
	Out.col  = AlbedoColor[In.ii % colSize];
	Out.matp = MaterialProperties[In.ii % matSize];
	
	return Out;
}
//______________________________________________________________________________


float4 PS(psInput In): SV_Target
{	
	
	uint colSize, matSize, lightSize, dummy;
    AlbedoColor.GetDimensions(colSize, dummy);
	MaterialProperties.GetDimensions(matSize, dummy);
	light.GetDimensions(lightSize, dummy);
	
	float4 col  = In.col;
	float4 mat = float4(In.matp,0,0);
	
	// to linear color space
	float3 albedo = pow(abs(col.rgb),2.2f);

	float3x3 tbn;
	tbn[2] = normalize(In.NormV);
		
	SurfaceProp p = {tbn, mat, In.vDirV, 1, albedo, disney};
	float3 result = 0;
	
	for (uint i = 0; i < lightSize; i++) 
	{
		if(light[i].type >= 0)
			result += GGX(p, light[i]);	
	}		

	result = tonemap ? ToneMapper(result) : result;
    return float4(result,col.a);
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
