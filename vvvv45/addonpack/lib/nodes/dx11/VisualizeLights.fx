//@author: flux
//@help: Physically based BRDF analytic lights
//@tags: helper, light volumes
//@credits: 

#define M_PI 3.14159265f
 
cbuffer cbPerDraw : register( b0 )
{
	float4x4 tVP : VIEWPROJECTION;	
	float4x4 tVI: VIEWINVERSE;
	float4x4 tV: VIEW;
};

cbuffer cbPerObj : register( b1 )
{
	float4x4 tW : WORLD;
	float Alpha <float uimin=0.0; float uimax=1.0;> = 1;
	int Id;
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

StructuredBuffer<LightBuffer> Light;

struct VS_IN
{
	float4 PosO : POSITION;
	uint ii : SV_InstanceID;
};

struct vs2ps
{
    float4 PosWVP: SV_POSITION;
	float3 col: COLOR0;
	float disabled : TEXCOORD0;
};

//______________________________________________________________________________

float3x3 lookat(float3 dir)
{    
	float3 up = float3(0,1,0)+1e-5f;
	float3 y=normalize(dir);
	float3 x=normalize(cross(up,y));
	float3 z=normalize(cross(y,x));
	return float3x3(x,y,z);
}
//______________________________________________________________________________

vs2ps VS(VS_IN In)
{
    vs2ps output;
	
	LightBuffer l = Light[In.ii];	
	
	int lType = l.type;
	l.ang =  floor(l.ang)/180;
	l.ang = lType==1 ? 1.0 : l.ang*0.5;
	l.ang = lType==0 ? 0.5 : l.ang;
	l.dir = normalize(l.dir);
	
///////////////////////////////TRANSFORM CONDITIONS/////////////////////////////
	
    if((Id > 1)){    	
    	In.PosO.xy = In.PosO.xy*l.ang-0.25f;  
    	In.PosO.xy = float2(cos(In.PosO.x*M_PI*2),sin(In.PosO.x*M_PI*2));
        In.PosO.y *= 1.0 -(Id == 3 && abs(In.PosO.y) == 1.0); 
    } 
	//__________________________________________________________________________
	
	 In.PosO.xyz = mul(float4(In.PosO.xyz,1),tW).xyz;
	//__________________________________________________________________________

	if((Id == 1)){			
		In.PosO.y = cos(M_PI*((In.PosO.y*0.5+0.5)*l.ang - 1.0));
		In.PosO.xz *= sqrt(1.0-In.PosO.y*In.PosO.y);
    }
	//__________________________________________________________________________
			
	if((Id == 0)){	
    	In.PosO.xyz = mul(In.PosO.xyz,(float3x3)tVI).xyz; 		
	}	
	//__________________________________________________________________________
	
	if(lType == 0){					
		
		l.dir = normalize(l.pos.xyz);	
		In.PosO.xyz += (Id == 0) ? l.dir*l.rad : 0;
		In.PosO.xyz *= (Id > 1) ? float3(0,l.rad,0) : 1;
		
		if(Id == 1){
			In.PosO.y = -l.rad;
			In.PosO.xz *= l.ang*0.5;
			In.PosO.xz *= (length(In.PosO.xz) < l.ang*0.49) ? 0 : sqrt(l.rad);
		}						
	}
	//__________________________________________________________________________	
	
   	if((Id != 0) && (lType != 1)){
   		float3x3 rotation = lookat(-l.dir);
    	In.PosO.xyz = mul(In.PosO.xyz,rotation);  	
	}		
	//__________________________________________________________________________

	In.PosO.xyz *= lerp(1.0,l.rad,(Id!=0)&&(lType!=0));      
    In.PosO.xyz += lType != 0 ? l.pos : 0;
	//__________________________________________________________________________
	
	output.PosWVP  = mul(In.PosO, tVP);
	output.col = l.col;
	output.disabled = lType;
    return output;
}

////////////////////////////////////////////////////////////////////////////////

float4 PS(vs2ps In): SV_Target
{
	clip(In.disabled);
    return float4(In.col.rgb, lerp(Alpha, 1, (Id == 0)));
}

////////////////////////////////////////////////////////////////////////////////

technique11 LightHelper
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetPixelShader( CompileShader( ps_5_0, PS() ) );
	}
}




