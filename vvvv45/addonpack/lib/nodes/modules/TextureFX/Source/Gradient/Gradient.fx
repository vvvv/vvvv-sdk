float2 R;
float2 FromXY;
float2 ToXY;
bool ClampColor;
float4 ColorA:COLOR;
float4 ColorB:COLOR;

texture tex0;
float4x4 GradientTransform:TEXTUREMATRIX;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 psDir(float2 x:TEXCOORD0):color{
    float4 c0=tex2D(s0,x);
    float4 grad=mul(float4(x.xy,0,1),GradientTransform);
	float fade=grad.x;
	if(ClampColor)fade=saturate(fade);
    float4 c=lerp(ColorA,ColorB,fade);
    return c;
}

float4 psGlow(float2 x:TEXCOORD0):color{
    float4 c0=tex2D(s0,x);
    float4 grad=mul(float4(x.xy,0,1),GradientTransform);
    float fade=length(grad.xy-.5);
	if(ClampColor)fade=saturate(fade);
	float4 c=lerp(ColorA,ColorB,fade);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Linear{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 psDir();}}
technique Radial{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 psGlow();}}
