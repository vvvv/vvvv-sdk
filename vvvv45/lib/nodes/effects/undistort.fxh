float2 tranf(float2 TexCd, float2 Trans, float2 Scale, float2 Resolution)
{
  return (TexCd * Scale + Trans) / Resolution;
};

float2 tranf_inv(float2 TexCd, float2 Trans, float2 Scale, float2 Resolution)
{
  return (TexCd * Resolution - Trans) / Scale;
};

float2 distort(float2 p, float k1, float k2, float p1, float p2)
{

    float sq_r = p.x*p.x + p.y*p.y;

    float2 q = p;
    float a = 1 + sq_r * (k1 + k2 * sq_r);
    float b = 2*p.x*p.y;

    q.x = a*p.x + b*p1 + p2*(sq_r+2*p.x*p.x);
    q.y = a*p.y + p1*(sq_r+2*p.y*p.y) + b*p2;

    return q;
}

float2 Undistort(float2 TexCd, float2 FocalLength, float2 PrincipalPoint, float4 Distortion, float2 Resolution)
{
	float2 scale = FocalLength;
	float2 trans = PrincipalPoint;
	
	float2 t = tranf_inv(TexCd, trans, scale, Resolution);
	t = distort(t, Distortion[0], Distortion[1], Distortion[2], Distortion[3]);
	t = tranf(t, trans, scale, Resolution);

	return t;
}

