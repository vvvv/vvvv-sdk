/*    HLSL port/modification of the code stolen from http://graphics.cs.williams.edu/papers/MedianShaderX6   */

/*
3x3 Median

GLSL 1.0
Morgan McGuire and Kyle Whitson, 2006
Williams Collevge
http://graphics.cs.williams.edu

Copyright (c) Morgan McGuire and Williams College, 2006
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
float2 R;
float Position <float uimin=0.0; float uimax=1.0;> =0.5;
int Radius <float uimin=1.0;> =1;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;AddressU=CLAMP;AddressV=CLAMP;};
#define s2(a, b)				temp = a; a = min(a, b); b = max(temp, b);
#define mn3(a, b, c)			s2(a, b); s2(a, c);
#define mx3(a, b, c)			s2(b, c); s2(a, c);
#define mnmx3(a, b, c)			mx3(a, b, c); s2(a, b);                                   // 3 exchanges
#define mnmx4(a, b, c, d)		s2(a, b); s2(c, d); s2(a, c); s2(b, d);                   // 4 exchanges
#define mnmx5(a, b, c, d, e)	s2(a, b); s2(c, d); mn3(a, c, e); mx3(b, d, e);           // 6 exchanges
#define mnmx6(a, b, c, d, e, f) s2(a, d); s2(b, e); s2(c, f); mn3(a, b, c); mx3(d, e, f); // 7 exchanges
float4 MEDIAN3x3(float2 vp:vpos):color{float2 x=(vp+.5)/R;
	float3 v[6];
	v[0] = tex2D(s0, x + Radius*float2(-1.0, -1.0)/R);
	v[1] = tex2D(s0, x + Radius*float2( 0.0, -1.0)/R);
	v[2] = tex2D(s0, x + Radius*float2(+1.0, -1.0)/R);
	v[3] = tex2D(s0, x + Radius*float2(-1.0,  0.0)/R);
	v[4] = tex2D(s0, x + Radius*float2( 0.0,  0.0)/R);
	v[5] = tex2D(s0, x + Radius*float2(+1.0,  0.0)/R);
	// Starting with a subset of size 6, remove the min and max each time
	float3 temp;
	mnmx6(v[0], v[1], v[2], v[3], v[4], v[5]);
	v[5] = tex2D(s0, x + float2(-1.0, +1.0)/R);
	mnmx5(v[1], v[2], v[3], v[4], v[5]);
	v[5] = tex2D(s0, x + float2( 0.0, +1.0)/R);
	mnmx4(v[2], v[3], v[4], v[5]);
	v[5] = tex2D(s0, x + float2(+1.0, +1.0)/R);
	mnmx3(v[3], v[4], v[5]);
	float4 c=tex2D(s0,x);
	//c.rgb=v[4];
	c.rgb=v[max(0,min(5,round(pow(Position+.001,.6)*5*.99)))];
    return c;
}

#define t2(a, b)				s2(v[a], v[b]);
#define t24(a, b, c, d, e, f, g, h)			t2(a, b); t2(c, d); t2(e, f); t2(g, h); 
#define t25(a, b, c, d, e, f, g, h, i, j)		t24(a, b, c, d, e, f, g, h); t2(i, j);

float4 MEDIAN5x5(float2 vp:vpos):color{float2 x=(vp+.5)/R;
	float3 v[25];
	for(int dX = -2; dX <= 2; ++dX) {
		for(int dY = -2; dY <= 2; ++dY) {		
			float2 offset = float2(float(dX), float(dY));
			v[(dX + 2) * 5 + (dY + 2)] = tex2D(s0, x + Radius*offset/R);
    	}
	}
	float3 temp;
	t25(0, 1,			3, 4,		2, 4,		2, 3,		6, 7);
	t25(5, 7,			5, 6,		9, 7,		1, 7,		1, 4);
	t25(12, 13,		11, 13,		11, 12,		15, 16,		14, 16);
	t25(14, 15,		18, 19,		17, 19,		17, 18,		21, 22);
	t25(20, 22,		20, 21,		23, 24,		2, 5,		3, 6);
	t25(0, 6,			0, 3,		4, 7,		1, 7,		1, 4);
	t25(11, 14,		8, 14,		8, 11,		12, 15,		9, 15);
	t25(9, 12,		13, 16,		10, 16,		10, 13,		20, 23);
	t25(17, 23,		17, 20,		21, 24,		18, 24,		18, 21);
	t25(19, 22,		8, 17,		9, 18,		0, 18,		0, 9);
	t25(10, 19,		1, 19,		1, 10,		11, 20,		2, 20);
	t25(2, 11,		12, 21,		3, 21,		3, 12,		13, 22);
	t25(4, 22,		4, 13,		14, 23,		5, 23,		5, 14);
	t25(15, 24,		6, 24,		6, 15,		7, 16,		7, 19);
	t25(3, 11,		5, 17,		11, 17,		9, 17,		4, 10);
	t25(6, 12,		7, 14,		4, 6,		4, 7,		12, 14);
	t25(10, 14,		6, 7,		10, 12,		6, 10,		6, 17);
	t25(12, 17,		7, 17,		7, 10,		12, 18,		7, 12);
	t24(10, 18,		12, 20,		10, 20,		10, 12);
	float4 c=tex2D(s0,x);
	//c.rgb=v[12];
	c.rgb = v[max(0,min(24,round(Position*24*.999)))];
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Median3x3{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 MEDIAN3x3();}}
technique Median5x5{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 MEDIAN5x5();}}