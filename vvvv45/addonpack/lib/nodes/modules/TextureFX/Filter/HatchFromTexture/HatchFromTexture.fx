//Credit : Digital Slaves

float2 R;
float Brightness = 2.5;
float Scale = 3.5;
float4 ColorA : COLOR <String uiname="Diffuse Color";>  = {1, 1, 1, 1};

texture Tex <string uiname="Texture";>;
texture Tex2 <string uiname="Hatch";>;

sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU  = Wrap;AddressV  = Wrap;};
sampler Samp2 = sampler_state  {Texture=(Tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU  = Wrap;AddressV  = Wrap;};

float4 p0(float2 vp : vpos): COLOR
{	
	float2 x=(vp+0.5)/R;
	
	float4 col = tex2D(Samp, x) ;
	
    float  c =(col.r + col.g + col.b) / 3.0 ;
	
	c *= Brightness;
	
	float3 col2 = tex2D(Samp2, x * Scale);
	
	float g0 = 0;
	float g1 = 0;
	float f  = 0;
	
	if( c >= 0.666)
	{
		f =(c - 0.666) / 0.333;
		g1 = 0;
		g0 = col2.r;
	}
	else if( c >= 0.333)
	{
		f =(c - 0.333) / 0.333;
		g1 = col2.r;
		g0 = col2.g;
	}
	else{
		f = c / 0.333;
		g1= col2.g;
		g0= col2.b;
	}
	
	float g = g0 + f *(g1 - g0);
	
	g = 1 - g;
  
    col = float4( g ,g ,g ,col.a ) * ColorA;
	
    return col;


}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique HatchFromTexture{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
