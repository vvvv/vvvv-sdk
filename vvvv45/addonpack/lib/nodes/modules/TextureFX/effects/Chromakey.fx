//@author: catweasel
//@help: draws a mesh with a constant color
//@tags: 
//@credits:

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
float hue;
float range;
float brightclip;
// --------------------------------------------------------------------------------------------------

//material properties
float4 cAmb : COLOR <String uiname="Color";>  = {1, 1, 1, 1};
float Alpha <float uimin=0.0; float uimax=1.0;> = 1;

//texture
texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
	Texture   = (Tex);          //apply a texture to the sampler
	MipFilter = LINEAR;         //sampler states
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

float4x4 tCol <string uiname="Color Transform";>;

//the data structure: vertexshader to pixelshader
//used as output data with the VS function
//and as input data with the PS function
struct vs2ps
{
	float4 Pos : POSITION;
	float4 TexCd : TEXCOORD0;
};

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 PS(vs2ps In): COLOR
{
	//In.TexCd = In.TexCd / In.TexCd.w; // for perpective texture projections (e.g. shadow maps) ps_2_0
	
	float4 col = tex2D(Samp, In.TexCd) * cAmb;
	col = mul(col, tCol);
	col.a *= Alpha;
	float r,g,b,delta;
	float colorMax, colorMin;
	float h=0,s=0,v=0;
	float4 hsv=0;
	
	r=col[0] ;
	g=col[1] ;
	b=col[2] ;
	
	colorMax = max (r,g);
	colorMax = max (colorMax,b);
	
	colorMin = min (r,g);
	colorMin = min (colorMin,b);
	
	v=colorMax;           //this is value
	
	if(colorMax !=0)
	{
		s=(colorMax-colorMin) / colorMax;
	}
	
	if (s != 0)    //if not achromatic
	{
		delta = colorMax - colorMin;
		if (r == colorMax)
		{
			h= (g-b)/delta ;
		}
		else if (g == colorMax)
		{
			h= 2.0 + (b-r) / delta;
		}
		else //b is max
		{
			h = 4.0 + (r-g) / delta;
		}
		
		h *= 60;
		
		if(h < 0)
		{
			h +=360;
		}
		
		hsv[0] = h/360;   //   moving h between 0 and 1
		hsv[1] =s;
		hsv[3] = v;
	}
	
	//return hsv;
	
	
	//||
	if (hsv[0] < (hue+ range) && hsv[0] > (hue- range)&& hsv[3] > brightclip )
	//&& hsv[1] > sat
	{
		col[3] =0  ;
	}
	
	return col;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique ChromaKey
{
	pass P0
	{
		//Wrap0 = U;  // useful when mesh is round like a sphere
		VertexShader = null;
		PixelShader = compile ps_2_0 PS();
	}
}

