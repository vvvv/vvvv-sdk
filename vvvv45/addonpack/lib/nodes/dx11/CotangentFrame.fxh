

float3x3 cotangent_frame(float3 n, float3 p, float2 uv)
{
    // get edge vectors of the pixel triangle
    float3 dp1 = ddx(p);
    float3 dp2 = ddy(p);
    float2 duv1 = ddx(uv);
    float2 duv2 = ddy(uv);
 
    // solve the linear system
    float3 dp2perp = cross(dp2, n);
    float3 dp1perp = cross(n, dp1);
    float3 t = dp2perp * duv1.x + dp1perp * duv2.x;
    float3 b = dp2perp * duv1.y + dp1perp * duv2.y;
 
    // construct a scale-invariant frame 
    float invmax = rsqrt(max(dot(t,t), dot(b,b)));
	
    return float3x3(normalize(t*invmax),-normalize(b*invmax),n);
}

//______________________________________________________________________________


float3x3 TBN_matrix(float3x3 TBN, float3 map, float bump)
{
	// get normalmapped TBN matrix   
	map = map * 255./127. - 128./127.;

	float3 Pn = normalize(mul(transpose(TBN), map));	
	float3 tmap = cross(map.xyz,float3(0,1,0));	
	float3 Pt = -mul(transpose(TBN), tmap);

	Pn = normalize(lerp(TBN[2],Pn,bump));	
	Pt = normalize(lerp(TBN[0],Pt,bump));	
	float3 Pb = normalize(cross(Pn,Pt));
	
	return float3x3(Pt,Pb,Pn);
}