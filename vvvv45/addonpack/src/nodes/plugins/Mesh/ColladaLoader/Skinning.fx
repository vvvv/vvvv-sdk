// SkinnedMesh.fx
// Effect processing for skinned mesh
// from SlimDX

float4 LightDirection	= {0.0f, 0.0f, -1.0f, 1.0f};
float4 LightDiffuse	= {0.6f, 0.6f, 0.6f, 1.0f};
float4 MaterialAmbient	= {0.1f, 0.1f, 0.1f, 1.0f};
float4 MaterialDiffuse	= {0.8f, 0.8f, 0.8f, 1.0f};

static const int MaxMatrices = 62;
float4x4 WorldMatrices[MaxMatrices];
float4x4 tVP : VIEWPROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;

struct VSInput
{
	float4 Position			: POSITION;
	float4 BlendWeights		: BLENDWEIGHT;
	int4   BlendIndices		: BLENDINDICES;
	float3 Normal			: NORMAL;
	float3 TextureCoordinates	: TEXCOORD0;
};

struct VSOutput
{
	float4 Position			: POSITION;
	float4 Diffuse			: COLOR0;
	float2 TextureCoordinates	: TEXCOORD0;
};

float3 Diffuse(float3 normal)
{
	float cosTheta;

	cosTheta = max(0.0f, dot(normal, LightDirection.xyz));

	return (cosTheta);
}

VSOutput VS(VSInput input, uniform int boneCount)
{
	VSOutput output = (VSOutput)0;

	float4 blendWeights = input.BlendWeights;
	int4 indices=input.BlendIndices;

	float4 pos = 0.0f;
	float4 norm = 0.0f;

        //position = input.Position;
        //normal = input.Normal;

	for (int i = 0; i < 2; i++)
	{
		pos = pos + (mul(input.Position, WorldMatrices[indices[i]]) * blendWeights[i]);
		norm = norm + (mul(input.Normal, WorldMatrices[indices[i]]) * blendWeights[i]);
	}
        /**/
	norm = normalize(norm);
      //  float4 pW = mul(input.Position, WorldMatrices[61]);

	output.Position = mul(pos, tWVP);

	output.Diffuse.xyz = MaterialAmbient.xyz + Diffuse(norm) * MaterialDiffuse.xyz;
	output.Diffuse.w = 1.0f;

	output.TextureCoordinates = input.TextureCoordinates.xy;

	return output;
}

technique SkinnedMesh
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS(2);
	}
}
