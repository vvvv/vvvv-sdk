float2 R;
float Width;
float Mask;
float Gamma=8;
#define PW (pow(2,Gamma))
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
       return pow(tex2D(s0,x),PW);
}
float2 SamplePos[32];
float4 p1(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 map=tex2D(s1,x);map=max(map.x,max(map.y,map.z))*map.a;if(!Mask)map=1;
    float lod=1+3*sqrt(map.x*saturate(pow(max(0.001,Width)*.1,.6))*log2(max(R.x,R.y)));
    float4 c=0;
    float kk=0;
    float2 diff=1+.05*dot(vp%2,float2(.25,.75));
    for (float i=0;i<12;i++){
        float2 off=map.x*SamplePos[i]*diff;
        c+=tex2Dlod(s0,float4(x+off,0,lod));
    }
    c=pow(c/12,1.);
    return c;
}
float4 p2(float2 vp:vpos):color{float2 x=(vp+.5)/R;
       return pow(tex2D(s0,x),1./PW);
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p2();}}
technique Wrap{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p2();}}
technique Mirror{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p2();}}
technique Border{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p2();}}
