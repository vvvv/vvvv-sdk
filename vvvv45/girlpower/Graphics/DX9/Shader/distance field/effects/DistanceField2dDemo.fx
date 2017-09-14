//@author: vvvv group
//@help: 2d distance field demo
//@tags: distanceField
//@credits: http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
 
#include "DistanceField2d.fxh" 

struct vs2ps
{
    float4 Pos : POSITION;
    float2 TexCd : TEXCOORD0;
};

vs2ps VS(
    float4 Pos : POSITION,
    float2 TexCd : TEXCOORD0)
{
    vs2ps Out = (vs2ps)0;
	Pos.xy *= 2;
    Out.Pos = Pos;
    Out.TexCd = TexCd;
    return Out;
}

float ShrinkAmount = 0.015;

float3 PreMultiplyAlpha(float4 color)
{
	return color.rgb * color.a;
}

float Patch1(float2 samplePos)
{
	float repeatUV = float2(5, 5);
	
	float2 repeatedLookupPos = RepeatedSampleBegin(samplePos, repeatUV);
	
	  float circles = Circle(float2(0.5, 0.5), 0.45, repeatedLookupPos);
	
	circles = RepeatedSampleEnd(circles, repeatUV);
	
	float triangle1 = Triangle(float2(0.3, 0.3), float2(0.2, 1), float2(1, 0.5), samplePos);

	float shape = Union(Substraction(triangle1, circles), Substraction(circles, triangle1));
	
	return shape;
}

float Patch2(float shape1, float2 samplePos)
{
	return Shrink(shape1, ShrinkAmount);
}

float Patch3(float shape2, float2 samplePos)
{
	return Outline(shape2, 0.02);
}

float Patch4(float shape3, float2 samplePos)
{
	return Outline(shape3, 0.0025);
}

float4 PS_Patch1(vs2ps In): COLOR
{
	float2 samplePos = In.TexCd;
	
	float a = ToAlpha(Patch1(samplePos));
	
	return float4(1, 1, 1, a);
}

float4 PS_Patch2(vs2ps In): COLOR
{
	float2 samplePos = In.TexCd;
	
	float shape1 = Patch1(samplePos);
	float shape2 = Patch2(shape1, samplePos);
	
	float a = ToAlpha(shape2);
	
	return float4(1, 1, 1, a);
}

float4 PS_Patch3(vs2ps In): COLOR
{
	float2 samplePos = In.TexCd;
	
	float shape1 = Patch1(samplePos);
	float shape2 = Patch2(shape1, samplePos);
	float shape3 = Patch3(shape2, samplePos);
	
	float a = ToAlpha(shape3);
	
	return float4(1, 1, 1, a);
}

float4 PS_Patch4(vs2ps In): COLOR
{
	float2 samplePos = In.TexCd;
	
	float shape1 = Patch1(samplePos);
	float shape2 = Patch2(shape1, samplePos);
	float shape3 = Patch3(shape2, samplePos);
	float shape4 = Patch4(shape3, samplePos);
	
	float a = 0.8 - (ToAlpha(shape4) * 0.9 + ToAlpha(shape3) * 0.15 - ToAlpha(shape2) * 0.03);
	
	return float4(1, 1, 1, a);
}

technique TShape1
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS_Patch1();
    }
}

technique TShape2
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS_Patch2();
    }
}

technique TShape3
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS_Patch3();
    }
}

technique TShape4
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS_Patch4();
    }
}