float2 R;
bool KeepSharp;
float2 Offset;
bool Filter=true;
bool Aspect=true;
float Zoom <float uimin=-1.0; float uimax=1.0;> = 0.0;
float Rotate <float uimin=-1.0; float uimax=1.0;> = 0.0;

float Alpha <float uimin=0.0; float uimax=1.0;> = 1.0;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
texture tex2;
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2p=sampler_state{Texture=(tex2);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;};
float mx(float3 p){return max(p.x,max(p.y,p.z));}
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}

float4 p0(float2 x:TEXCOORD0):color{float2 asp=lerp(1,R.x/R,Aspect);
    float4 c=tex2D(s0,x);
    float2 dx=r2d((x-.5)/asp,Rotate)*asp*pow(2,Zoom)+.5+Offset/R;
    float4 pre=tex2D(s2p,dx);if(Filter)pre=tex2D(s2,dx);
    c=float4(lerp(pre,c,lerp(1,c.a,Alpha)).rgb,pre.a+c.a);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[1]=CLAMP;AddressV[1]=CLAMP;AddressU[2]=CLAMP;AddressV[2]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[1]=WRAP;AddressV[1]=WRAP;AddressU[2]=WRAP;AddressV[2]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[1]=MIRROR;AddressV[1]=MIRROR;AddressU[2]=MIRROR;AddressV[2]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[1]=BORDER;AddressV[1]=BORDER;AddressU[2]=BORDER;AddressV[2]=BORDER;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
