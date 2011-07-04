float2 R;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;
float V;
texture tex0,tex1,tex2,tex3;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 psJoin(float2 x:TEXCOORD0):color{
    float4 c0=tex2D(s0,x);
    float4 c1=tex2D(s1,x);
    float4 c2=tex2D(s2,x);
    //float4 c=lerp(c0,c1,saturate(c2-1+2*Fader));
    float4 c=lerp(c0,c1,lerp(lerp(0,c2,saturate(Fader*2)),1,saturate(Fader*2-1)));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique TJoin{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 psJoin();}}
