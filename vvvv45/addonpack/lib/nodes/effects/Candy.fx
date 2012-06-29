//@author: unc
//@help: metallic/porcelain-like material, using diffuse and reflection cubemap
//@tags: cubemap


//globals
float4x4 tW: WORLD;
float4x4 tV: VIEW;
float4x4 tP: PROJECTION;
float4x4 tVP: VIEWPROJECTION;
float3 posCam : CAMERAPOSITION;
int2 R <string uiname="Cubemap Resolution";> =512;

//textures
texture texENVI <string uiname="Reflect CubeMap";>;
samplerCUBE sENVI=sampler_state{Texture=(texENVI);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
texture texDIFF <string uiname="Diffuse CubeMap";>;
samplerCUBE sDIFF=sampler_state{Texture=(texDIFF);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

texture texAO <string uiname="LightMap";>;
sampler sAO=sampler_state{Texture=(texAO);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

/*

texture texNOR <string uiname="NormalMap";>;
sampler sNOR=sampler_state{Texture=(texNOR);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

texture texCOL <string uiname="ColorMap";>;
sampler sCOL=sampler_state{Texture=(texCOL);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

texture texREF <string uiname="SpecularMap";>;
sampler sREF=sampler_state{Texture=(texREF);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
*/
struct vs2ps
{
    float4 PosWVP:POSITION;
	float4 PosW:COLOR0;
	float4 TexCd:COLOR1;
	float3 NormO:NORMAL0;
	float4 PosO:TEXCOORD2;
};

float3 Center=float3(0,0,0);
float4 DiffuseColor:COLOR <string uiname="Diffuse Color";> = 0.7;
float4 ReflectColor:COLOR <string uiname="Reflect Color";> = 0.4;
float4 DiffuseGamma:COLOR <string uiname="Diffuse Gamma";> = 0.0;
float4 ReflectGamma:COLOR <string uiname="Reflect Gamma";> = 0.3;
float ReflectionBlur <float uimin=0.0;float uimax=1.0;string uiname="Reflection Blur";> = 0.5;
float Metallic <float uimin=0.0;string uiname="Metallic Shade";> = 0.1;
float ShadowGamma <float uimin=0.0;float uimax=1.0;string uiname="Ambient Occlusion Gamma";> = 0.5;
float Alpha <float uimin=0.0;float uimax=1.0;> =1.0;
float3 ColGamma(float3 c,float3 a,float pv=1){
	//c.rgb=(c.rgb-.4)*(a)+.4;
	c.rgb=normalize(c.rgb)*pow(length(c.rgb)*pv,a)/pv;
	return c.rgb;
}
float linstep(float a,float b,float x){return saturate((x-a)/(b-a));}
float mx(float3 x){return max(x.x,max(x.y,x.z));}
float4 LevelBlendDD(samplerCUBE s,float3 x,float lod){
	x=x+Center;
	float MaxLOD=log2(max(R.x,R.y));
	lod+=pow(2,3*mx(fwidth(normalize(x)).xyz));
	float4 c=0;
	float3 cx=x/max(length(x.x),max(length(x.y),length(x.z)));
	c+=texCUBElod(s,float4(x.xyz,lod));
	float ed=(lod-1)/MaxLOD;
	ed=1-ed*ed*ed*.5;
	if(!(mx(abs(cx.xyy))<ed||mx(abs(cx.xzz))<ed||mx(abs(cx.zyy))<ed)){
		c+=float4(texCUBElod(s,float4(x.zyx*float3( 1, 1, 1),lod)).xyz,1)*linstep( ed, 1,cx.z)*linstep( ed, 1,cx.x);
		c+=float4(texCUBElod(s,float4(x.zyx*float3( 1, 1, 1),lod)).xyz,1)*linstep(-ed,-1,cx.z)*linstep(-ed,-1,cx.x);
		c+=float4(texCUBElod(s,float4(x.zyx*float3(-1, 1,-1),lod)).xyz,1)*linstep( ed, 1,cx.z)*linstep(-ed,-1,cx.x);
		c+=float4(texCUBElod(s,float4(x.zyx*float3(-1, 1,-1),lod)).xyz,1)*linstep(-ed,-1,cx.z)*linstep( ed, 1,cx.x);
		c+=float4(texCUBElod(s,float4(x.xzy*float3( 1, 1, 1),lod)).xyz,1)*linstep( ed, 1,cx.z)*linstep( ed, 1,cx.y);
		c+=float4(texCUBElod(s,float4(x.xzy*float3( 1, 1, 1),lod)).xyz,1)*linstep(-ed,-1,cx.z)*linstep(-ed,-1,cx.y);
		c+=float4(texCUBElod(s,float4(x.xzy*float3( 1,-1,-1),lod)).xyz,1)*linstep( ed, 1,cx.z)*linstep(-ed,-1,cx.y);
		c+=float4(texCUBElod(s,float4(x.xzy*float3( 1,-1,-1),lod)).xyz,1)*linstep(-ed,-1,cx.z)*linstep( ed, 1,cx.y);
		c+=float4(texCUBElod(s,float4(x.yxz*float3( 1, 1, 1),lod)).xyz,1)*linstep( ed, 1,cx.x)*linstep( ed, 1,cx.y);
		c+=float4(texCUBElod(s,float4(x.yxz*float3( 1, 1, 1),lod)).xyz,1)*linstep(-ed,-1,cx.x)*linstep(-ed,-1,cx.y);
		c+=float4(texCUBElod(s,float4(x.yxz*float3(-1,-1, 1),lod)).xyz,1)*linstep( ed, 1,cx.x)*linstep(-ed,-1,cx.y);
		c+=float4(texCUBElod(s,float4(x.yxz*float3(-1,-1, 1),lod)).xyz,1)*linstep(-ed,-1,cx.x)*linstep( ed, 1,cx.y);
	}
	c.xyz/=c.a;
	c.a=1;
	return c;
}

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------
vs2ps VS_Candy(
    float4 PosO: POSITION,
    float4 NormalO: NORMAL,
	float4 TexCd:TEXCOORD0){
    vs2ps Out=(vs2ps)0;
	Out.PosO=PosO;
    Out.PosW=mul(PosO,tW);
	Out.NormO=normalize(NormalO);
    Out.PosWVP=mul(mul(Out.PosW,tV),tP);
	Out.TexCd=TexCd;
    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------
float4 PS_Candy(vs2ps In): COLOR{
	
	float MaxLOD=log2(max(R.x,R.y));
	//"Ray" direction vector
	float3 cd=normalize(mul(float4(In.PosO.xyz,1),tW).xyz-posCam);
	//Surface normal in world space
	float3 nr=normalize(mul(float4(In.NormO.xyz,0),tW).xyz);
	//Gamma values
	float4 AG=ShadowGamma/(1.0001-ShadowGamma);
	//AG=AG/pow(DiffuseColor+.2,1);
	float4 DG=DiffuseGamma*.5+.5;DG=DG/(1.00001-DG);
	float4 RG=ReflectGamma*.5+.5;RG=RG/(1.00001-RG);
	
	//Ambient occlusion
    float4 ao=tex2Dlod(sAO,float4(In.TexCd.x,1-In.TexCd.y,0,1));
	
	//Metallic shade	
	float glow=dot(cd,-nr);
	
	//Read from reflection map with lod "blur" and edge blending (to cover seams on low mip levels)
	float4 cReflect = LevelBlendDD(sENVI, reflect(cd,nr)+Center,1+ReflectionBlur*MaxLOD);	
	//Read from diffuse map
	float4 cDiffuse = texCUBE(sDIFF,nr);

	//Add metallic shade
	cDiffuse.rgb*=pow(glow,5*Metallic/pow(DiffuseColor.rgb+.000001,.5))*pow(2,Metallic);
	
	//Apply some kind of gammacorrection to diffuse and reflect colors
	cDiffuse.rgb=ColGamma(cDiffuse.rgb+.00001,DG,1.1);
	cReflect.rgb=ColGamma(cReflect.rgb,RG,1.2);
	
	//Apply ambient occlusion shading (with gammacorrection for AO)
	cDiffuse.rgb*=pow(ao,AG);
	cReflect.rgb*=pow(ao,.2*AG);
	
	//Add reflect and diffuse together
	float4 c=1;
	c.xyz=DiffuseColor*cDiffuse.rgb+ReflectColor*cReflect.rgb;
	c.a=Alpha;

    return c;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------
technique TCandy
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Candy();
        PixelShader  = compile ps_3_0 PS_Candy();
    }
}
