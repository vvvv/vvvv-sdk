//globals
float4x4 tW: WORLD;
float4x4 tV: VIEW;
float4x4 tP: PROJECTION;
float4x4 tVP: VIEWPROJECTION;
float3 posCam : CAMERAPOSITION;
float2 TR=512.0;
float2 R=512.0;
//textures
texture tex0;
sampler s2D=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
samplerCUBE sCB=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s3D=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

int Type;
bool Background=0;
int CH=0;
//sampler3D sENVI=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

//float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;
#include "ColorSpace.fxh"
struct vs2ps
{
    float4 PosWVP:POSITION;
    float3 ViewVectorW:TEXCOORD0;
    float3 NormW:TEXCOORD1;
	
	float4 PosW:COLOR0;
	float4 TexCd:COLOR1;
	float3 NormO:NORMAL0;
	float4 PosO:TEXCOORD2;
};

float linstep(float a,float b,float x){return saturate((x-a)/(b-a));}
float mx(float3 x){return max(x.x,max(x.y,x.z));}

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------
float4 ShowChannels(float4 c){
	switch(CH){
		case 0: {return c;break;}
		case 1: {return float4(c.rgb,1);break;}
		case 2: {return float4(c.rrr,c.a);break;}
		case 3: {return float4(c.ggg,c.a);break;}
		case 4: {return float4(c.bbb,c.a);break;}
		case 5: {return float4(c.aaa,1);break;}
		case 6: {return float4(RGBtoHSL(c.rgb).xxx,1);break;}
		case 7: {return float4(RGBtoHSL(c.rgb).yyy,1);break;}
		case 8: {return float4(RGBtoHSL(c.rgb).zzz,1);break;}
		case 9: {return float4(RGBtoHSV(c.rgb).zzz,1);break;}
	}
	return c;
}
vs2ps VS_Preview(
    float4 PosO: POSITION,
    float4 NormalO: NORMAL,
	float4 TexCd:TEXCOORD0)
{
    vs2ps Out=(vs2ps)0;
	Out.PosO=PosO;
    Out.PosW=mul(PosO,tW);
    Out.ViewVectorW=Out.PosW - posCam;
    Out.NormW=normalize(mul(float4(NormalO.xyz,0),tW));
	Out.NormO=normalize(NormalO);
    Out.PosWVP=mul(mul(Out.PosW,tV),tP);
	Out.TexCd=TexCd;
    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------


float4 PS_Preview(vs2ps In): COLOR
{
	float MaxLOD=log2(max(R.x,R.y));
	float3 cd=normalize(mul(float4(In.PosO.xyz,1),tW).xyz-posCam);
	float3 nr=normalize(mul(float4(In.NormO.xyz,0),tW).xyz);
	float3 cp=reflect(cd,nr);
	if(Background)cp=In.PosW.xyz*In.PosW.w;
	float4 c = texCUBE(sCB, float4(cp,1));
	if(Type==0)c=tex2D(s2D,In.TexCd);
	if(Type==2)c=tex3D(s3D,In.PosO+.5);
	
	c=ShowChannels(c);
	
    return c;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TPreview
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Preview();
        PixelShader  = compile ps_3_0 PS_Preview();
    }
}
