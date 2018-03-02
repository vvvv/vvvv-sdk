//@author: vvvv group
//@help: Effect processing for skinned mesh with directional light.
//@tags: skeleton, bones, collada, shading, blinn
//@credits: SlimDX

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tWV: WORLDVIEW;
float4x4 tWVP: WORLDVIEWPROJECTION;
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (EX9)

static const int MaxMatrices = 60;
float4x4 JointMatrices[MaxMatrices];

#include <effects\PhongDirectional.fxh>

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

struct VSInput
{
	float4 Position			: POSITION;
	float4 BlendWeights		: BLENDWEIGHT;
	int4   BlendIndices		: BLENDINDICES;
	float3 Normal			: NORMAL;
	float3 TextureCoordinates	: TEXCOORD0;
};

struct vs2ps
{
    float4 PosWVP: POSITION;
    float4 TexCd : TEXCOORD0;
    float3 LightDirV: TEXCOORD1;
    float3 NormV: TEXCOORD2;
    float3 ViewDirV: TEXCOORD3;
};

vs2ps VS(
	float4 PosO: POSITION,
    float3 NormO: NORMAL,
    float4 TexCd : TEXCOORD0,
	float4 BlendWeights	: BLENDWEIGHT,
	int4   BlendIndices	: BLENDINDICES)
{
	//inititalize all fields of output struct with 0
	vs2ps Out = (vs2ps)0;

    //---------- Skinning ----------
	float4 pos = 0;
	float3 norm = 0;

    for (int i = 0; i < 4; i++)
    {
        pos = pos + mul(PosO, JointMatrices[BlendIndices[i]]) * BlendWeights[i];
    	norm = norm + mul(NormO, JointMatrices[BlendIndices[i]]) * BlendWeights[i];
    }
    //-------- End Skinning --------

    //inverse light direction in view space
    Out.LightDirV = normalize(-mul(lDir, tV));
    
    //normal in view space
    Out.NormV = normalize(mul(norm, tWV));

    //position (projected)
    Out.PosWVP  = mul(pos, tWVP);
    Out.TexCd = mul(TexCd, tTex);
    Out.ViewDirV = -normalize(mul(PosO, tWV));
    return Out;
}

float4 PS(vs2ps In)  : COLOR
{
    //In.TexCd = In.TexCd / In.TexCd.w; // for perpective texture projections (e.g. shadow maps) ps_2_0

    float4 col = tex2D(Samp, In.TexCd);

    col.rgb *= PhongDirectional(In.NormV, In.ViewDirV, In.LightDirV);

    return mul(col, tColor);
}

technique SkinnedMesh
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}
