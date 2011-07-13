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
texture tex1;
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
texture tex2;
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
sampler s2p=sampler_state{Texture=(tex2);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;AddressU=WRAP;AddressV=WRAP;};
texture tex3;
sampler s3=sampler_state{Texture=(tex3);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float mx(float3 p){return max(p.x,max(p.y,p.z));}
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}

float4 pNORMAL(float2 x:TEXCOORD0):color{float2 asp=lerp(1,R.x/R,Aspect);
    float4 c=tex2D(s0,x)*float4(1,1,1,Alpha);
    float2 dx=r2d((x-.5)/asp,Rotate)*asp*pow(2,Zoom)+.5+Offset/R;
    float4 pre=tex2D(s2p,dx);if(Filter)pre=tex2D(s2,dx);
    c=float4(lerp(pre,c,c.a).rgb,pre.a+c.a);
    return c;
}
float4 pINVERT(float2 x:TEXCOORD0):color{float2 asp=lerp(1,R.x/R,Aspect);
    float4 c=tex2D(s0,x)*float4(1,1,1,Alpha);
    float2 dx=r2d((x-.5)/asp,Rotate)*asp*pow(2,Zoom)+.5+Offset/R;
    float4 pre=tex2D(s2p,dx);if(Filter)pre=tex2D(s2,dx);
    c=float4(lerp(1-pre,c,c.a).rgb,pre.a+c.a);
    return c;
}
float4 pADD(float2 x:TEXCOORD0):color{float2 asp=lerp(1,R.x/R,Aspect);
    float4 c=tex2D(s0,x);
    float2 dx=r2d((x-.5)/asp,Rotate)*asp*pow(2,Zoom)+.5+Offset/R;
    float4 pre=tex2D(s2p,dx);if(Filter)pre=tex2D(s2,dx);
    c=float4((pre*Alpha+c).rgb,pre.a+c.a);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Normal{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pNORMAL();}}
technique Invert{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pINVERT();}}
technique Add{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pADD();}}
