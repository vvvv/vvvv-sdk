float2 R;
float M[8];
#define PW M[7]
#define GlowPower M[0]
#define GlowShape M[1]
#define GlowWidth M[5]
#define GlowGamma M[6]
#define PostGamma M[2]
#define PostBright M[3]
#define PostSatur M[4]

#define puw(a,b) normalize(a)*pow(length(a),b)
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
float mx(float3 p){return max(p.x,max(p.y,p.z));}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    //c=lerp(c,1-c,(PostBright<0));
    c.xyz=puw(c.xyz,GlowGamma);
    return c;
}
float4 p1(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2Dlod(s0,float4(x,0,3));
    float s=dot(c.rgb,1.)/3.;
    s=mx(c.rgb);
    c.r=pow(abs(s),PW);
    c.g=pow(1./abs(s),PW);
    c.b=abs(s);
    return c;
}
float4 p2(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2Dlod(s1,float4(x,0,88));
    float cmax=pow(c.r,1./PW);
    float cmin=pow(1./c.g,1./PW);
    float cavg=c.b;
    float4 bm=0;
    for (float i=0;i<7;i++){
        float mip=log2(R.y)*.2*i*GlowWidth+1.5;
        float3 nc=tex2Dlod(s0,float4(x+(vp%2-.5).yx/R*pow(2,i+.3)*3*GlowWidth*GlowWidth,0,mip)).xyz;
        nc=(nc);
        bm+=float4(nc,1)*pow(2,i*GlowShape)/float4((1+bm/37).xyz,1);
    }
    bm=bm/bm.a;
    c.xyz=puw(bm.xyz,2);
    c.xyz*=pow(cmax/(cavg+.3),.85)/pow(cavg,.35);
    c=saturate(c/8)*8;
    return c;
}
float4 p3(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2Dlod(s1,float4(x,0,1));c.xyz=puw(c.xyz,1./GlowGamma);
    float4 bm=tex2Dlod(s0,float4(x,0,2));
    float bma=dot(tex2Dlod(s0,88).xyz,1)/3.;
    //bm.r=tex2Dlod(s0,float4(x+float2(1,0)/R*pow(bma*8,.7),0,2)).r;
    //bm.b=tex2Dlod(s0,float4(x-float2(1,0)/R*pow(bma*8,.7),0,2)).b;
    bm=bm*GlowPower/pow(bma+.5,.8);
    c.xyz=normalize(c.xyz)*puw(length(c.xyz),1.2+pow(dot(bm.xyz,1)/3.*.5,2))+puw(bm.xyz,1);
    c=c/pow(.6+4*bma,.5);
    c=saturate(c*PostBright);
    c.xyz=dot(c.xyz,1.)/3.+PostSatur*(c.xyz-dot(c.xyz,1.)/3.);
    c=pow(saturate(c),PostGamma);
    c.a=tex2Dlod(s1,float4(x,0,1)).a;
    return c;
}
void vs2d(inout float4 vp:POSITION0){vp.xy*=2;}
technique tc0{
//MipLodBias[0]=-1.0;MipLodBias[1]=-1.0;vertexshader=NULL;
    pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}
    pass pp1{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}
    pass pp2{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p2();}
    pass pp3{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p3();}
}
