//first version by Digital Slaves
//adapted as a texture filter by lecloneur

float2 R;
float width <float uimin=0.0; string uiname="Line Width";> = 10.0;
float4 ColorA:COLOR <String uiname="Background Color";>  = {1, 1, 1, 1};
float4 ColorB:COLOR <String uiname="Color";>  = {0, 0, 0, 1};

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{	
	float2 x=(vp+0.5)/R;
	float a = width/R.x;
	float b = width/R.y;
	float4 c1 = tex2D(Samp, x);
	float4 c2 = tex2D(Samp, x + float2(a,0));
	float4 c3 = tex2D(Samp, x + float2(0,b));  
	float f = 0;
	f += abs(c1.x-c2.x);
	f += abs(c1.y-c2.y);
	f += abs(c1.z-c2.z); 
	f += abs(c1.x-c3.x);
	f += abs(c1.y-c3.y);
	f += abs(c1.z-c3.z);
	f -= 0.2;  
	f = saturate(f);  
	c1.rgb = ColorA.rgb * (1-f) + ColorB.rgb * f;
	return c1;
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique Charcoal{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
