float2 R;

float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;
float frequency = 20.0;
float speed = 10.0;
float amplitude = 0.05;

texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0 (float2 x : TEXCOORD0) : COLOR
{
	float2 center = float2(0.5,0.5);
	float2 toUV = x - center;
	float distanceFromCenter = length(toUV);
	float2 normToUV = toUV / distanceFromCenter;

	float wave = cos(frequency * distanceFromCenter - speed * Fader);
	float offset1 = Fader * wave * amplitude;
	float offset2 = (1.0 - Fader) * wave * amplitude;
	
	float2 newUV1 = center + normToUV * (distanceFromCenter + offset1);
	float2 newUV2 = center + normToUV * (distanceFromCenter + offset2);
	
	float4 c1 = tex2D(s0, newUV1); 
    float4 c2 = tex2D(s1, newUV2);

	return lerp(c1, c2, Fader);
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Ripple{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
