float2 R;
float fuzzyAmount <float uimin=0.0;> = 0.1;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;

texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 uv : TEXCOORD0) :COLOR
{
	float radius = -fuzzyAmount + Fader * (0.70710678 + 2.0 * fuzzyAmount);
	float fromCenter = length(uv - float2(0.5,0.5));
	float distFromCircle = fromCenter - radius;
	
	float4 c1 = tex2D(s0, uv); 
    float4 c2 = tex2D(s1, uv);
    
	float p = saturate((distFromCircle + fuzzyAmount) / (2.0 * fuzzyAmount));
	return lerp(c2, c1, p);
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique CircleReveal{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
