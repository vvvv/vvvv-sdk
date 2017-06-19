float2 R;
float2 Scale;
float OffsetX <float uimin=-1.0; float uimax=1.0;> = 0.0;
float OffsetY <float uimin=-1.0; float uimax=1.0;> = 0.0;
float Rotate;
bool Aspect;
bool Filter;
float4 BorderCol:COLOR ={0.0,0.0,0.0,0.0};
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;};

float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}

float4 p0(float2 x:TEXCOORD0):color{float2 asp=lerp(1,R.x/R,Aspect);
    float2 vp=x*R-.25;
    float2 dx=r2d((x-.5-float2(OffsetX,OffsetY))/asp,Rotate)/Scale*asp+.5;
    float4 c=tex2D(s1,dx);
    if(Filter)c=tex2D(s0,dx);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;AddressU[1]=CLAMP;AddressV[1]=CLAMP;vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;AddressU[1]=WRAP;AddressV[1]=WRAP;vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;AddressU[1]=MIRROR;AddressV[1]=MIRROR;vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;AddressU[1]=BORDER;AddressV[1]=BORDER;BorderColor[1]=BorderCol;vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
