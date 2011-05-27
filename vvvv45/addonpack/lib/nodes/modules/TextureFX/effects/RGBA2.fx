float2 R;
bool4 V;
texture tex0,tex1,tex2,tex3;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s3=sampler_state{Texture=(tex3);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
struct col2{float4 c0:COLOR0;float4 c1:COLOR1;float4 c2:COLOR2;float4 c3:COLOR3;};
col2 psSplit(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    col2 RGBA=(col2)0;
    RGBA.c0=c*float4(1,0,0,1);
    RGBA.c1=c*float4(0,1,0,1);
    RGBA.c2=c*float4(0,0,1,1);
    RGBA.c3=c.a;
    return RGBA;
}
float4 psJoin(float2 x:TEXCOORD0):color{
       if(!any(V))return 0;
    float4 c0=tex2D(s0,x)*V.r;
    float4 c1=tex2D(s1,x)*V.g;
    float4 c2=tex2D(s2,x)*V.b;
    float4 c3=tex2D(s3,x)*V.a;//if(!V.a)c3=1;
    //c3=float4(0,0,0,max(c3.r,max(c3.g,c3.b)));
    c3=float4(0,0,0,c3.a);
    if(!V.a)c3.a=max(c3.a,max(c0.a,max(c1.a,c2.a)));
    c0=float4(1,0,0,0)*max(c0.x,max(c0.y,c0.z));
    c1=float4(0,1,0,0)*max(c1.x,max(c1.y,c1.z));
    c2=float4(0,0,1,0)*max(c2.x,max(c2.y,c2.z));
    float4 c=c0+c1+c2+c3;
    return c;
}
technique TSplit{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 psSplit();}}
technique TJoin{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 psJoin();}}
