float2 R;
float time;
float4 ColorA:COLOR <String uiname="Background Color";>  = {0, 0, 0, 1};
float4 ColorB:COLOR<String uiname="Lava Color";>  = {1, 1, 1, 1};
float phi = 0.0;

float3 mod289(float3 x)
{
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float4 mod289(float4 x)
{
  return x - floor(x  *(1.0 / 289.0)) * 289.0;
}

float4 permute(float4 x)
{
  return mod289(((x *34.0)+1.0)*x);
}

float4 taylorInvSqrt(float4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r * phi*5.0;
}

float3 fade(float3 t) 
{
  return t*t*t*(t*(t*6.0-15.0)+10.0);
}

float cnoise(float3 P)
{
	float3 Pi0 = floor(P);
	float3 Pi1 = Pi0 + float3(1.0, 1.0, 1.0);
	Pi0 = mod289(Pi0);
	Pi1 = mod289(Pi1);
	float3 Pf0 = frac(P);
	float3 Pf1 = Pf0 - float3(1.0, 1.0, 1.0);
	float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
	float4 iy = float4(Pi0.yy, Pi1.yy);
	float4 iz0 = Pi0.zzzz;
	float4 iz1 = Pi1.zzzz;
	
	float4 ixy = permute(permute(ix) + iy);
	float4 ixy0 = permute(ixy + iz0);
	float4 ixy1 = permute(ixy + iz1);

	float4 gx0 = ixy0 * (1.0 / 7.0);
	float4 gy0 = frac(floor(gx0) * (1.0 / 7.0)) - 0.5;
	gx0 = frac(gx0);
	float4 gz0 = float4(0.5, 0.5, 0.5, 0.5) - abs(gx0) - abs(gy0);
	float4 sz0 = step(gz0, float4(0.0, 0.0, 0.0, 0.0));
	gx0 -= sz0 * (step(0.0, gx0) - 0.5);
	gy0 -= sz0 * (step(0.0, gy0) - 0.5);

	float4 gx1 = ixy1 * (1.0 / 7.0);
	float4 gy1 = frac(floor(gx1) * (1.0 / 7.0)) - 0.5;
	gx1 = frac(gx1);
	float4 gz1 = float4(0.5, 0.5, 0.5, 0.5) - abs(gx1) - abs(gy1);
	float4 sz1 = step(gz1, float4(0.0, 0.0, 0.0, 0.0));
	gx1 -= sz1 * (step(0.0, gx1) - 0.5);
	gy1 -= sz1 * (step(0.0, gy1) - 0.5);

	float3 g000 = float3(gx0.x,gy0.x,gz0.x);
	float3 g100 = float3(gx0.y,gy0.y,gz0.y);
	float3 g010 = float3(gx0.z,gy0.z,gz0.z);
	float3 g110 = float3(gx0.w,gy0.w,gz0.w);
	float3 g001 = float3(gx1.x,gy1.x,gz1.x);
	float3 g101 = float3(gx1.y,gy1.y,gz1.y);
	float3 g011 = float3(gx1.z,gy1.z,gz1.z);
	float3 g111 = float3(gx1.w,gy1.w,gz1.w);

	float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
	g000 *= norm0.x;
	g010 *= norm0.y;
	g100 *= norm0.z;
	g110 *= norm0.w;
	float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
	g001 *= norm1.x;
	g011 *= norm1.y;
	g101 *= norm1.z;
	g111 *= norm1.w;

	float n000 = dot(g000, Pf0);
	float n100 = dot(g100, float3(Pf1.x, Pf0.yz));
	float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
	float n110 = dot(g110, float3(Pf1.xy, Pf0.z));
	float n001 = dot(g001, float3(Pf0.xy, Pf1.z));
	float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
	float n011 = dot(g011, float3(Pf0.x, Pf1.yz));
	float n111 = dot(g111, Pf1);

	float3 fade_xyz = fade(Pf0);
	float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
	float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
	float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x); 
	return 2.2 * n_xyz;
}

float surface3 ( float3 coord ) 
{
	float frequency = 4.0;
	float n = 0.0;		
	n += 1.0	* abs( cnoise( coord * frequency ) );
	n += 0.5	* abs( cnoise( coord * frequency * 2.0 ) );
	n += 0.25	* abs( cnoise( coord * frequency * 4.0 ) );
	n += 0.125	* abs( cnoise( coord * frequency * 8.0 ) );
	n += 0.0625	* abs( cnoise( coord * frequency * 16.0 ) );
	n += 0.03125 * abs( cnoise( coord * frequency * 32.0 ) );
	return n;
}

float4 p0(float2 vp:vpos):color 
{
	float2 position = vp.xy / R.xy;
	float n = surface3(float3(position, time * 0.1));	
	float2 uv = position;
    float lum = length(n);
	float4 tc = 1.- float4(1.-lum, 1.-lum, 1.-lum, 1-lum);
	//float4 tc = 1.-pow(float4(1.-lum, 1.-lum, 1.-lum, 1-lum), ColorA);
	return lerp (ColorA, ColorB, tc);
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Lava{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
