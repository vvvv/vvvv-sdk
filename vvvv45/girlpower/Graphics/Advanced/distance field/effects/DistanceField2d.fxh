// http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm

float Circle(float2 center, float radius, float2 samplePos)
{
	return length(samplePos - center) - radius;
}

float Triangle(float2 a, float2 b, float2 c, float2 samplePos)
{
	float2 ab = b-a;
	float2 ac = c-a;
	float2 bc = c-b;
	float2 as = samplePos-a;
	float2 bs = samplePos-b;
	
	float2 nab = normalize(float2(-ab.y, ab.x));
	float2 nac = normalize(float2(ac.y, -ac.x));
	float2 nbc = normalize(float2(-bc.y, bc.x));
	
	float u = dot(as, nab);
	float v = dot(as, nac);
	float w = dot(bs, nbc);
	
	return max(max(u, v), w);
}

float Union(float d1, float d2)
{
	return min(d1, d2);
}

float Substraction(float d1, float d2 )
{
    return max(d1, -d2);
}

float Intersection(float d1, float d2 )
{
    return max(d1, d2);
}

float Outline(float d, float radius)
{
	return abs(d) - radius;
}

float Shrink(float d, float amount)
{
	return d + amount;
}

float Grow(float d, float amount)
{
	return d - amount;
}

float4 FillWithColorAntialised(float d, float4 color)
{
    return float4(color.rgb, saturate(-d * 500) * color.a);
}

float4 ToAlpha(float d)
{
    return saturate(-d * 500);
}

float2 RepeatedSampleBegin(float2 samplePos, float2 n)
{
    return (samplePos % (1/n))*n;
}

float2 RepeatedSampleEnd(float d, float2 n)
{
    return d/n;
}

// objects need to be placed within (0,0) .. (1/n.x, 1/n.y)
float2 SimpleRepeater(float2 samplePos, float2 n)
{
    return (samplePos % (1/n));
}

//many more trivial functions can be thought of

//halfplane -> one half of the plane is inside the other half is outside.
//             hint: take another outline function to get a line

//line -> a line on the plane

//linesegment -> like line, but ends on A, B

//rectangle 

//...