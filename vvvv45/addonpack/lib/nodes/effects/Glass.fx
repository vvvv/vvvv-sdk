//@author: unc
//@help: Glass material, reflection and refraction from cubemap
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
//texture texREFR <string uiname="Refract CubeMap";>;
samplerCUBE sREFR=sampler_state{Texture=(texENVI);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};


struct vs2ps
{
    float4 PosWVP:POSITION;
	float4 PosW:COLOR0;
	float4 TexCd:COLOR1;
	float3 NormO:NORMAL0;
	float4 PosO:TEXCOORD2;
};

float3 Center=float3(0,0,0);
float4 RefractColor:COLOR <string uiname="Refract Color";> = 1.0;
float4 ReflectColor:COLOR <string uiname="Reflect Color";> = 1.0;
float4 RefractGamma:COLOR <string uiname="Refract Gamma";> = 0.0;
float4 ReflectGamma:COLOR <string uiname="Reflect Gamma";> = 0.0;
float ReflectionBlur <float uimin=0.0;float uimax=1.0;string uiname="Reflection Blur";> = 0.25;
float RefractionBlur <float uimin=0.0;float uimax=1.0;string uiname="Refraction Blur";> = 0.5;
float Transparency <float uimin=0.0;float uimax=1.0;> =1.0;
float RefractionIndex <float uimin=0.0;float uimax=2.0;> =0.75;
float FreshnelExponent <float uimin=0.0;> =1;
float Dispersion=.2;
bool EdgeFix <string uiname="Dispersion or edge fix";> =0;

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
	lod+=pow(2,4*mx(fwidth(normalize(x)).xyz))-1;
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
vs2ps VS_Glass(
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

float4 PS_Glass(vs2ps In): COLOR{
	
	float MaxLOD=log2(max(R.x,R.y));
	//"Ray" direction vector
	float3 cd=normalize(mul(float4(In.PosO.xyz,1),tW).xyz-posCam);
	//Surface normal in world space
	float3 nr=normalize(mul(float4(In.NormO.xyz,0),tW).xyz);
	//Gamma values
	float4 DG=RefractGamma*.5+.5;DG=DG/(1.00001-DG);
	float4 RG=ReflectGamma*.5+.5;RG=RG/(1.00001-RG);

	//Read from reflection map with lod "blur" and edge blending (to cover seams on low mip levels)
	float4 cReflect = LevelBlendDD(sENVI, reflect(cd,nr)+Center,1+ReflectionBlur*MaxLOD);
	
	//Refraction stuff
	float glow=dot(cd,-nr);
	float f=.1+.9*pow(glow,FreshnelExponent);
	float4 cRefract=1;
	float rlod=pow(RefractionBlur+.000001,1+pow(glow,2)*2)*MaxLOD;
	cRefract.r=texCUBElod(sREFR,float4(refract(cd,nr,RefractionIndex*pow(1.01,-Dispersion))+Center,1+rlod)).r;
	cRefract.g=texCUBElod(sREFR,float4(refract(cd,nr,RefractionIndex)+Center,1+rlod)).g;
	cRefract.b=texCUBElod(sREFR,float4(refract(cd,nr,RefractionIndex*pow(1.01,Dispersion))+Center,1+rlod)).b;
	//edge blending - too slow to calculate with Dispersion
	if(EdgeFix)cRefract = LevelBlendDD(sREFR, refract(cd,nr,RefractionIndex),1+rlod);
	
	//Apply some kind of gammacorrection to refract and reflect colors
	cRefract.rgb=ColGamma(cRefract.rgb+.00001,DG,1.1);
	cReflect.rgb=ColGamma(cReflect.rgb,RG,1.2);

	//Blend colors together
	float4 c=1;
	c.xyz=lerp(ReflectColor*cReflect,RefractColor*cRefract,f*Transparency);
	c.a=Alpha;

    return c;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------
technique TGlass
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Glass();
        PixelShader  = compile ps_3_0 PS_Glass();
    }
}
