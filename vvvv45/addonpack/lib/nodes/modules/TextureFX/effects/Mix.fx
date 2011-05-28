float2 R;
float Fader;
float V;
texture tex0,tex1,tex2,tex3;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 psJoin(float2 x:TEXCOORD0):color{
    float4 c0=tex2D(s0,x);
    float4 c1=tex2D(s1,x);
    float4 c2=tex2D(s2,x);
    float4 c=lerp(c0,c1,saturate(lerp(Fader,c2,V)));
    return c;
}
technique TJoin{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 psJoin();}}
