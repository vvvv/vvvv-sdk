float2 R;

float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0 (float2 uv : TEXCOORD0) : COLOR
{
	float4 c1 = tex2D(s0, uv);
    float4 c2 = tex2D(s1, uv);
    return lerp(c1,c2, Fader);
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Fade{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
