//@author: vvvv group
//@help: draws a mesh with a constant color
//@tags: template, basic
//@credits:

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//transforms
float4x4 tW: WORLD;        //the models world matrix
float4x4 tV: VIEW;         //view matrix as set via Renderer (EX9)
float4x4 tP: PROJECTION;   //projection matrix as set via Renderer (EX9)
float4x4 tWVP: WORLDVIEWPROJECTION;

float3 LightPos <string uiname="Light Position";> = 0;
float bias <string uiname="Shadow Bias";> = 0.02;
float contrast <string uiname="Contrast";> = 5;
const int samples <string uiname="Samples";> = 3;
float softness <string uiname="Softness";> = 0;
float blurring <string uiname="Softness Distance Multiplier";> = 1;

//texture
texture Tex <string uiname="Texture";>;
texture TexMap <string uiname="ShadowMap";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
samplerCUBE smap = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (TexMap);          //apply a texture to the sampler
    MipFilter = LINEAR;         //sampler states
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

//the data structure: vertexshader to pixelshader
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
    float4 Pos : POSITION;
    float4 TexCd : TEXCOORD0;
    float4 wPos : TEXCOORD1;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------

vs2ps VS_shadows(
    float4 Pos : POSITION,
    float4 TexCd : TEXCOORD0)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    //transform position
    Out.Pos = mul(Pos, tWVP);

    //transform texturecoordinates
    Out.TexCd = mul(TexCd, tTex);
	Out.wPos = mul(Pos,tW);

    return Out;
}
vs2ps VS_Distance(
	float4 Pos: POSITION,
    float4 TexCd : TEXCOORD0)
{
   vs2ps Out = (vs2ps) 0;
   Out.Pos = mul(Pos, tWVP);
   Out.TexCd = mul(TexCd, tTex);
   Out.wPos = mul(Pos,tW);
   return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------
struct outcolor
{
	float4 c1 : COLOR0;
	float4 c2 : COLOR1;
};
outcolor PS_shadows(vs2ps In): COLOR
{
    //In.TexCd = In.TexCd / In.TexCd.w; // for perpective texture projections (e.g. shadow maps) ps_2_0
	outcolor col = (outcolor) 0;
	float3 pp1 = In.wPos.xyz;
	float3 pp2 = pp1 - LightPos;
	float pdist1 = texCUBE(smap, normalize(pp2)).x;
    float pdist2 = sqrt(pp2.x*pp2.x+pp2.y*pp2.y+pp2.z*pp2.z);
	float dd = pdist2 - pdist1;
	col.c2.xyz = dd;
	col.c1.xyz=1;
	float3 offs = -softness;
	for(int i=0; i<samples; i++) {
		offs.y = -softness;
		for(int j=0; j<samples; j++) {
			offs.x = -softness;
			for(int k=0; k<samples; k++) {
				float3 p1 = In.wPos.xyz+offs*(pdist2*blurring);
				float3 p2 = p1 - LightPos;
				float dist1 = texCUBE(smap, normalize(p2)).x;
    			float dist2 = sqrt(p2.x*p2.x+p2.y*p2.y+p2.z*p2.z);
				//float tmp = ((dist2-bias) > dist1);
				float tmp = min(max((dist2-dist1)*contrast-bias,0),1);
				col.c1.xyz -= tmp/pow(samples,3);
				offs.x += (softness/samples)*2;
			}
			offs.y += (softness/samples)*2;
		}
		offs.z += (softness/samples)*2;
	}
	
	//col.c1.xyz = ((dist2-bias) > dist1) ? 0 : 1;
	float4 t = tex2D(Samp, In.TexCd);
	col.c1.a = t.a;
	col.c2.a = t.a;
    return col;
}
float4 PS_Distance(vs2ps In) : COLOR
{
	float3 p1 = In.wPos.xyz;
	float3 p2 = p1 - LightPos;
    float dist = sqrt(p2.x*p2.x+p2.y*p2.y+p2.z*p2.z);
    return float4(dist.xxx, tex2D(Samp, In.TexCd).a);
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique shadows
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_3_0 VS_shadows();
        PixelShader = compile ps_3_0 PS_shadows();
    }
}
technique distance
{
    pass P0
    {
        //Wrap0 = U;  // useful when mesh is round like a sphere
        VertexShader = compile vs_3_0 VS_Distance();
        PixelShader = compile ps_3_0 PS_Distance();
    }
}