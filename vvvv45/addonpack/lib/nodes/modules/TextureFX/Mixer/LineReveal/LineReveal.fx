float2 R;
float2 lineOrigin = 1.0;
float2 lineNormal = 1.0;
float2 lineOffset = 0.0;
float fuzzyAmount <float uimin=0.0;> = 0.0;

float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0 (float2 uv : TEXCOORD0) : COLOR
{
	float2 currentLineOrigin = lerp(lineOrigin, lineOffset, Fader);
	float2 normLineNormal = normalize(lineNormal);
	float4 c1 = tex2D(s0, uv);
    float4 c2 = tex2D(s1, uv);
    
	float distFromLine = dot(normLineNormal, uv-currentLineOrigin);
	float p = saturate((distFromLine + fuzzyAmount) / (2.0 * fuzzyAmount));
	return lerp(c2, c1, p);
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique LineReveal{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
