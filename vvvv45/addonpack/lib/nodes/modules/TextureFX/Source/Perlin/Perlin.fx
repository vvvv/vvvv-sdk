float2 R;
float OctaveCount;
float Seed;
float3 Offset;
float Amplify;
float Balance;
float2 Sizes[13];
texture tex0,tex1,tex2,tex3,tex4,tex5,tex6,tex7,tex8,tex9,tex10,tex11,tex12;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s3=sampler_state{Texture=(tex3);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s4=sampler_state{Texture=(tex4);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s5=sampler_state{Texture=(tex5);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s6=sampler_state{Texture=(tex6);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s7=sampler_state{Texture=(tex7);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s8=sampler_state{Texture=(tex8);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s9=sampler_state{Texture=(tex9);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s10=sampler_state{Texture=(tex10);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s11=sampler_state{Texture=(tex11);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s12=sampler_state{Texture=(tex12);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float el(sampler s,float2 x,float Octave){
      x=frac(x+Octave*.145+Offset.xy);
      x-=.5/R;
      float2 cR=Sizes[Octave-1];
      x*=(cR-1)/cR;
      x+=.5/cR;
      float c=tex2D(s,x).x;
      c*=saturate(OctaveCount-Octave);
      //c*=(Octave<=OctaveCount);
      return c;
}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
    c+=el(s1,x,1);
    c+=el(s2,x,2);
    c+=el(s3,x,3);
    c+=el(s4,x,4);
    c+=el(s5,x,5);
    c+=el(s6,x,6);
    c+=el(s7,x,7);
    c+=el(s8,x,8);
    c+=el(s9,x,9);
    c+=el(s10,x,10);
    c+=el(s11,x,11);
    c+=el(s12,x,12);
    c=c.r;
    c=c.r*Amplify*.5*pow(10,Balance)+.5;
//c=el(s12,x,12).r*.25+.5;
    c=c.r;
    c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Perlin{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
