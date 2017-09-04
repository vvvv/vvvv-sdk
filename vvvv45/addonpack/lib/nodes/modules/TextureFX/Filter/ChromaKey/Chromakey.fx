float2 R;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float hue;
float range;
float brightclip;

float4 PS(float2 x:TEXCOORD0): COLOR
{
	float4 col = tex2D(s0, x);
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
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique ChromaKey{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 PS();}}
