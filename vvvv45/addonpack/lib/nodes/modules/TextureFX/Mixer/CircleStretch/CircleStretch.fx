float2 R;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;

texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float DistanceFromCenterToSquareEdge(float2 dir)
{
	dir = abs(dir);
	float dist = dir.x > dir.y ? dir.x : dir.y;
	return dist;
}

float4 p0(float2 uv : TEXCOORD0) :COLOR
{
	float2 center = float2(0.5,0.5);
	float radius = Fader * 0.70710678;
	float2 toUV = uv - center;
	float len = length(toUV);
	float2 normToUV = toUV / len;
	
	if(len < radius)
	{
		float distFromCenterToEdge = DistanceFromCenterToSquareEdge(normToUV) / 2.0;
		float2 edgePoint = center + distFromCenterToEdge * normToUV;
	
		float minRadius = min(radius, distFromCenterToEdge);
		float percentFromCenterToRadius = len / minRadius;
		
		float2 newUV = lerp(center, edgePoint, percentFromCenterToRadius);
		return tex2D(s1, newUV);
	}
	else
	{
		float distFromCenterToEdge = DistanceFromCenterToSquareEdge(normToUV);
		float2 edgePoint = center + distFromCenterToEdge * normToUV;
		float distFromRadiusToEdge = distFromCenterToEdge - radius;
		
		float2 radiusPoint = center + radius * normToUV;
		float2 radiusToUV = uv - radiusPoint;
		
		float percentFromRadiusToEdge = length(radiusToUV) / distFromRadiusToEdge;
		
		float2 newUV = lerp(center, edgePoint, percentFromRadiusToEdge);
		return tex2D(s0, newUV);
	}

}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique CircleStretch{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
