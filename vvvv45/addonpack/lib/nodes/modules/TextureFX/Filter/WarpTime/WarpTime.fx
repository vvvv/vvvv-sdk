float2 R;
float Smooth <float uimin=1.0; float uimax=32.0;> = 3.0;
float Position;
float Part;
float Amount;
texture tex0,tex1,tex2,tex3,tex4,tex5,tex6,tex7,tex8,tex9,texA,texB,texC,texD,texE,Mask;
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
sampler sA=sampler_state{Texture=(texA);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler sB=sampler_state{Texture=(texB);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler sC=sampler_state{Texture=(texC);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler sD=sampler_state{Texture=(texD);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler sE=sampler_state{Texture=(texE);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler sM=sampler_state{Texture=(Mask);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 fd(float4 c,float k,float2 x,float sm){
       float num=k+Part*15.0;
       //c*=smoothstep(1./80.,0,abs(sm-num/45.));
       //c*=step(abs(sm-num/(Amount-1)),.5/Amount);
       sm=lerp(.5,sm,1./pow(1.1,Smooth-1));
       float li=(sm-(num)/(Amount-1));
       //li=max(0,li);
       //if(num!=0&&num!=Amount-1)li=abs(li);
       //()
       li=abs(li);
      // if(num==0)c.a=1;
       c*=smoothstep(Smooth/(Amount-1),0,li)/Smooth;
      // if(num==0){c*=Smooth/2;}

       //c=abs(sm-num/45.)<.45/45.;
       //sm=1;
      return c;
}
float4 p0(float2 x:TEXCOORD0):color{
    float2 vp=x*R-.25;
    float4 c=0;
    float sm=tex2D(sM,x);
    //sm=x.y;
    c+=fd(tex2D(s0,x),0,x,sm);
    c+=fd(tex2D(s1,x),1,x,sm);
    c+=fd(tex2D(s2,x),2,x,sm);
    c+=fd(tex2D(s3,x),3,x,sm);
    c+=fd(tex2D(s4,x),4,x,sm);
    c+=fd(tex2D(s5,x),5,x,sm);
    c+=fd(tex2D(s6,x),6,x,sm);
    c+=fd(tex2D(s7,x),7,x,sm);
    c+=fd(tex2D(s8,x),8,x,sm);
    c+=fd(tex2D(s9,x),9,x,sm);
    c+=fd(tex2D(sA,x),10,x,sm);
    c+=fd(tex2D(sB,x),11,x,sm);
    c+=fd(tex2D(sC,x),12,x,sm);
    c+=fd(tex2D(sD,x),13,x,sm);
    c+=fd(tex2D(sE,x),14,x,sm);
    //c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique WarpTime{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
