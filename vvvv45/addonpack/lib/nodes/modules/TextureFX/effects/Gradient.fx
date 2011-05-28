float2 R;
float2 Direction;
float4 ColorA:COLOR;
float4 ColorB:COLOR;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 psJoin(float2 x:TEXCOORD0):color{
    float4 c0=tex2D(s0,x);
    float4 c=lerp(ColorA,ColorB,saturate(dot(x-.5,Direction)+.5));
    return c;
}
technique Gradient{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 psJoin();}}
