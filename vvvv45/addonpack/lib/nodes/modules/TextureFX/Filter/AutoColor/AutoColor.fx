float2 R;
float PW <float uimin=1.0; float uimax=32.0;> = 15.0;
float AutoMax <float uimin=0.0; float uimax=1.0;> = 1.0;
float AutoGamma <float uimin=0.0; float uimax=1.0;> = 0.0;
float fade <float uimin=0.0; float uimax=1.0;> = 0.0;
float Reset=0;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):COLOR0{float2 x=(vp+.5)/R;
       float4 u=tex2D(s0,x);
       float4 c=tex2D(s0,x);
       c.x=pow(max(u.r,max(u.g,u.b)),PW);
       c.y=dot(u.rgb,1)/3.;
       c.z=0;
       float4 pre=tex2D(s1,x);
       c.x=pow(lerp(pow(c.x,1./PW),pow(pre.x,1./PW),fade),PW);
       c.y=lerp(c.y,pre.y,fade*!Reset);
       return c;
}
float4 p1(float2 vp:vpos):COLOR0{float2 x=(vp+.5)/R;
       float4 m=tex2Dlod(s1,float4(x,0,99));
       float cmax=pow(m.x,1./PW);
       float cmin=m.z;
       float cavg=m.y;
       cmax=lerp(1,cmax,AutoMax);
       cavg=(cavg-cmin)/max(cmax-cmin,.00000001);
       float4 c=(tex2D(s0,x)-cmin)/max(cmax-cmin,.00000001);
       c.rgb=sign(c.rgb)*pow(abs(c.rgb),pow(.5/cavg,AutoGamma));
       c.a=tex2D(s0,x).a;
       return c;
}
float4 p2(float2 vp:vpos):COLOR0{float2 x=(vp+.5)/R;
       float4 c=tex2Dlod(s1,float4(x,0,1));
       return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique t{

          pass P0{vertexshader=compile vs_3_0 vs2d();PixelShader=compile ps_3_0 p0();}
          pass P1{vertexshader=compile vs_3_0 vs2d();PixelShader=compile ps_3_0 p1();}
          pass P1{vertexshader=compile vs_3_0 vs2d();PixelShader=compile ps_3_0 p2();}
}
