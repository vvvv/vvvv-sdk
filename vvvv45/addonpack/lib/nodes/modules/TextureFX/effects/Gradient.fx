float2 R;
float2 Direction;
float4 ColorA:COLOR;
float4 ColorB:COLOR;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 psDir(float2 x:TEXCOORD0):color{
    float4 c0=tex2D(s0,x);
    float4 c=saturate(lerp(ColorA,ColorB,dot(x-.5,Direction)+.5));
    return c;
}
float4 psGlow(float2 x:TEXCOORD0):color{
    float4 c0=tex2D(s0,x);
    float4 c=saturate(lerp(ColorA,ColorB,1-length(x-.5-Direction)));
    return c;
}
technique Linear{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 psDir();}}
technique Glow{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 psGlow();}}
