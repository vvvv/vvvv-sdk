float2 R;
bool Alpha;
float Bright <float uimin=0.0;> = 1.0;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
texture tex1;
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);float pa=c.a;
    c=c-tex2D(s1,x);
    c=abs(c*pow(2,Bright));

    if(!Alpha)c.a=pa;

    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique FrameDiff{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
