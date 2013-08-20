float2 R;
float2 R2;
float Speed <float uimin=-1.0; float uimax=1.0;> = 0.1;
float Fade <float uimin=0.0; float uimax=1.0;> = 0.9;
texture tex0,tex1,texFeed;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler sFeed=sampler_state{Texture=(texFeed);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 x:TEXCOORD0,float2 x2:TEXCOORD1):color{
    float2 dx=tex2D(s1,x2).xy;
    dx=lerp(x,dx,Speed);
    float4 c=lerp(tex2D(s0,dx),tex2D(sFeed,dx),saturate(Fade));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,out float2 uv2:TEXCOORD1){vp.xy*=2;uv2=uv+.5/R2;uv+=.5/R;}
technique DisplaceEcho{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
