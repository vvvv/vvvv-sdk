//FXAA by Timothy Lottes
//Fxaa3_8.h taken from http://timothylottes.blogspot.com/2011/06/fxaa3-source-released.html
#define FXAA_PC 1
#define FXAA_HLSL_3 1
#define FXAA_LINEAR 1
#include "Fxaa3_8.h"
float2 R;
float2 rcpFrame;
float4 rcpFrameOpt;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 x:TEXCOORD0,float2 pos:TEXCOORD1,float4 posPos:TEXCOORD2):color{
    float4 c=tex2D(s0,x);
    return float4(c.rgb,dot(c.rgb,float3(0.299, 0.587, 0.114)));
}
float4 p1(float2 x:TEXCOORD0,float2 pos:TEXCOORD1,float4 posPos:TEXCOORD2):color{
    return float4(FxaaPixelShader(pos,posPos,s0,rcpFrame,rcpFrameOpt).xyz,tex2D(s1,x).a);
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,out float2 pos:TEXCOORD1,out float4 posPos:TEXCOORD2){vp.xy*=2;posPos=uv.xyxy+float2(0,1).xxyy/R.xyxy;uv+=.5/R;pos=uv;}
technique LightnessToAlpha{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique FXAA{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}}

