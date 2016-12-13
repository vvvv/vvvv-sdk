float2 R;
float amount = 1.0;
float randomSeed<float uimin=0.0; float uimax=1.0;> = 0.5;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;

texture tex0,tex1,tex2;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 uv : TEXCOORD0) :COLOR
{
	float2 offset = (tex2D(s2, float2(uv.x/10.0, frac(uv.y/10.0 + min(0.9, randomSeed)))).xy * (amount*10.0) - 1.0);
	float2 center = uv + offset/100.0;
	float2 toUV = uv - center;
	float len = length(toUV);
	float2 normToUV = toUV / len;
	float angle = atan2(normToUV.y,normToUV.x);
	
	angle += 3.141592*2.0*Fader;
	float2 newOffset;
	sincos(angle,newOffset.y, newOffset.x); 
	newOffset *= len;
	
	float4 c1 = tex2D(s0, frac(center + newOffset));
    float4 c2 = tex2D(s1, frac(center + newOffset));

	return lerp(c1, c2, Fader);
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Crumble{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
