float2 R;
float4 Levels <float uimin=1.0;> = (4.0,4.0,4.0,4.0);
bool Alpha;
float Dither <float uimin=0.0;>;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);float pa=c.a;
    c=floor(c*max(Levels,0)+Dither*dot(vp%2,float2(.75,.25))*frac(c*max(Levels,0)))/(max(Levels,0)+.000000001);
    if(!Alpha)c.a=pa;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Posterize{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
