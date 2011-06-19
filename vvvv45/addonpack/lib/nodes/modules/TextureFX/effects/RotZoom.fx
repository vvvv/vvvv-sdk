float2 R;
float2 Scale;
float2 Offset;
float Rotate;
bool Filter;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;};

float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}

float4 p0(float2 x:TEXCOORD0):color{
    float2 vp=x*R-.25;
    float2 dx=r2d(x-.5-Offset,Rotate)/Scale+.5;
    float4 c=tex2D(s1,dx);
    if(Filter)c=tex2D(s0,dx);
    return c;
}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;pixelshader=compile ps_2_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;pixelshader=compile ps_2_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;pixelshader=compile ps_2_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;pixelshader=compile ps_2_0 p0();}}
