float2 R;
float randomSeed;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;

texture tex0,tex1,tex2;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 uv : TEXCOORD0) :COLOR
{
	float2 center = float2(0.5,0.5);
	float2 toUV = uv - center;
	float distanceFromCenter = length(toUV);
	float2 normToUV = toUV / distanceFromCenter;
	float angle = (atan2(normToUV.y, normToUV.x) + 3.141592) / (2.0 * 3.141592);
	float offset1 = tex2D(s2, float2(angle, frac(Fader/3 + distanceFromCenter/5 + randomSeed))).x * 2.0 - 1.0;
	float offset2 = offset1 * 2.0 * min(0.3, (1-Fader)) * distanceFromCenter;
	offset1 = offset1 * 2.0 * min(0.3, Fader) * distanceFromCenter;
	
	float4 c1 = tex2D(s0, frac(center + normToUV * (distanceFromCenter + offset1))); 
    float4 c2 = tex2D(s1, frac(center + normToUV * (distanceFromCenter + offset2)));

	return lerp(c1, c2, Fader);
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique RadialWiggle{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
