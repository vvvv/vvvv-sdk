//@author: vvvv group
//@help: Effect processing for skinned mesh with directional light.
//@tags: skeleton, bones, collada, shading
//@credits: SlimDX

float4 LightDirection	= {0.0f, 0.0f, -1.0f, 1.0f};
float4 LightDiffuse	= {0.6f, 0.6f, 0.6f, 1.0f};
float4 MaterialAmbient	= {0.1f, 0.1f, 0.1f, 1.0f};
float4 MaterialDiffuse	= {0.8f, 0.8f, 0.8f, 1.0f};

static const int MaxMatrices = 60;
float4x4 SkinningMatrices[MaxMatrices];
float4x4 tW: WORLD;
float4x4 tVP : VIEWPROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;

//texture
texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
float4x4 tColor <string uiname="Color Transform";>;

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

	cosTheta = max(0.0f, dot(normal, -LightDirection.xyz));

	return (cosTheta);
}

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;

        /*
         * ---------- Skinning ----------
         */
	float4 blendWeights = input.BlendWeights;
	int4 indices = input.BlendIndices;
	
	float4 pos = 0;
	float3 norm = 0;

        for (int i = 0; i < 4; i++)
        {
            pos = pos + mul(input.Position, SkinningMatrices[indices[i]]) * blendWeights[i];
	    norm = norm + mul(input.Normal, SkinningMatrices[indices[i]]) * blendWeights[i];
        }
        /*
         * -------- End Skinning --------
         */

	norm = normalize(norm);

	output.Position = mul(pos, tWVP);
	output.Diffuse.xyz = MaterialAmbient.xyz + Diffuse(norm) * MaterialDiffuse.xyz;
	output.Diffuse.w = 1.0f;
	output.TextureCoordinates = input.TextureCoordinates.xy;

	return output;
}


float invlerp (float x, float SourceMin, float SourceMax)
{
        return (x - SourceMin) / (SourceMax - SourceMin);
}

float3 invlerp (float3 x, float SourceMin, float SourceMax)
{
        return (x - SourceMin) / (SourceMax - SourceMin);
}


float4 PS(VSOutput input)  : COLOR
{

    float4 col = tex2D(Samp, input.TextureCoordinates);
    col.rgb *= input.Diffuse;
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
