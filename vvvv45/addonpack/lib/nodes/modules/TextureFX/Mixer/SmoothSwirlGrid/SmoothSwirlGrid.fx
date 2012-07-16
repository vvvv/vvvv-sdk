float2 R;
float twistAmount = 1.0;
int cellcount <int uimin=1;> = 7;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;

texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 SampleWithBorder(float4 border, sampler2D tex, float2 uv)
{
	if (any(saturate(uv) - uv))
	{
		return border;
	}
	else
	{
		return tex2D(tex, uv);
	}
}

float4 p0(float2 uv : TEXCOORD0) : COLOR
{
	float cellsize = 1.0 / cellcount;
	
	float2 cell = floor(uv * cellcount);
	float2 oddeven = fmod(cell, 2.0);
	float cellTwistAmount = twistAmount*5;
	if (oddeven.x < 1.0)
	{
		cellTwistAmount *= -1;
	}
	if (oddeven.y < 1.0)
	{
		cellTwistAmount *= -1;
	}
	
	float2 newUV = frac(uv * cellcount);
	
	float2 center = float2(0.5,0.5);
	float2 toUV = newUV - center;
	float distanceFromCenter = length(toUV);
	float2 normToUV = toUV / distanceFromCenter;
	float angle = atan2(normToUV.y, normToUV.x);
	
	angle += max(0, 0.5-distanceFromCenter) * cellTwistAmount * Fader;
	float2 newUV2;
	sincos(angle, newUV2.y, newUV2.x);
	newUV2 *= distanceFromCenter;
	newUV2 += center;
	
	newUV2 *= cellsize;
	newUV2 += cell * cellsize;
	
	float4 c1 = tex2D(s0, newUV2);
    float4 c2 = tex2D(s1, uv);

    return lerp(c1,c2, Fader);
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique SmoothSwirlGrid{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
