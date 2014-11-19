float2 R;
float Count;
float Index;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);AddressU=WRAP;AddressV=WRAP;MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s0p=sampler_state{Texture=(tex0);AddressU=WRAP;AddressV=WRAP;MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;};

float4 psHS(float2 x:TEXCOORD0):color{
    float c=8./Count/Count;
    return c;
}
void vsHS(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,out float ps:PSIZE){vp.xy=vp.xy*2/Count-1;ps=1;
float4 c=tex2Dlod(s0p,float4(vp.xy*.5+.5,0,1));
vp.xy=float2(c.b*2-1,0);}

float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2Dlod(s0,float4(x,0,1));
    c.g=pow(c.g,12);
    return c;
}
float4 p1(float2 x:TEXCOORD0):color{
    float4 c=tex2Dlod(s0,float4(x,0,1));
    c=.75*c.r/pow(tex2Dlod(s0,33).g,1./12.);
    return c;
}
float4 pGRAPH(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
    c=(1-x.y-c.r);
    c=step(c,0)*lerp(.9,1,step(-c,1./R.y));
    c.a=1;
    //c+=12*abs(tex2D(s0,x)-tex2D(s0,x+float2(1,0)/R));
    //c=max(step(c,0)*.85,smoothstep(.01+8.*pow(fwidth((c)),2),0,abs(c)));
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}

technique Histogram{pass pp0{PointSpriteEnable=TRUE;vertexshader=compile vs_3_0 vsHS();pixelshader=compile ps_3_0 psHS();}}
technique Normalizer{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}pass pp1{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}}
technique Graph{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pGRAPH();}}
