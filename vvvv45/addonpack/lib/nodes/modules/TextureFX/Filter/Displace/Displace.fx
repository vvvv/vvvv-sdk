float2 R;
float2 R2;
float2 Amount=(1,1);
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 x:TEXCOORD0,float2 x2:TEXCOORD1):color{
    float4 c=tex2D(s0,lerp(x,tex2D(s1,x2).xy,Amount));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,out float2 uv2:TEXCOORD1){vp.xy*=2;uv2=uv+.5/R2;uv+=.5/R;}
technique Displace{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
