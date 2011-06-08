float2 R;
float4 ColorA:COLOR;
float4 ColorB:COLOR;
float SrcRgbAmount <float uimin=0.0; float uimax=8.0;> = 1;
float SrcRgbGamma <float uimin=0.0; float uimax=8.0;> = 1;
float Brightness <float uimin=-1.0; float uimax=8.0;> = 0;
float Gamma <float uimin=-1.0; float uimax=8.0;> = 0;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    float grey=dot(c.rgb,1)/3.;
    float3 rgb=(c.rgb-grey);
    c.rgb=grey*pow(2,Brightness)*lerp(lerp(ColorA,ColorB,smoothstep(-.4,.4,grey-.4)),1,pow(grey,2));
    c.rgb=c.rgb+sign(rgb)*pow(abs(rgb)*sqrt(2)*SrcRgbAmount,pow(2,SrcRgbGamma));
    c.rgb=pow(c.rgb,pow(2,Gamma));
    return c;
}
technique PhotoFilm{pass pp0{vertexshader=null;pixelshader=compile ps_2_0 p0();}}
