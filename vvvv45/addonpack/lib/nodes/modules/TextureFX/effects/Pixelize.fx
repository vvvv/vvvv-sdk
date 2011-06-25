float2 R;
int2 PixelSize <float uimin=0;> = (16,16);
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 x:TEXCOORD0):color{
    float2 vp=x*R-.25;
    float2 sz=min(max(0.5/R,PixelSize),R);
    float4 c=tex2D(s0,floor(vp/sz)*sz/R+.5/R);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Pixelize{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
