float2 R;
float Fader <float uimin=0.0; float uimax=1.0;> = 0.5;
float2 Move;
float Zoom;
texture tex0,tex1,tex2,tex3;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 x:TEXCOORD0):color{

    float4 c0=tex2D(s0,x);
    float4 c1=tex2D(s1,x);
    float2 x0=(x-.5)*pow(2,c1.z*Zoom)+.5+Move*2*normalize((c1.xy-.5))*pow(length((c1.xy-.5)),0.7);
    float2 x1=(x-.5)/pow(2,c0.z*Zoom)+.5+Move*2*normalize((c0.xy-.5))*pow(length((c0.xy-.5)),0.7);
    float4 a0=tex2D(s0,lerp(x,x0,Fader));
    float4 a1=tex2D(s1,lerp(x1,x,Fader));
    float4 c=lerp(a0,a1,Fader);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Morph{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
