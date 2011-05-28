float2 R;
float Width;
float Balance;
float SharpEdges;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
    float sharp=0;
    float bal=.5;
    sharp=max(sharp,fwidth(length(tex2Dlod(s0,float4(x,0,5)).xyz)))*pow(2,Balance/1.);
    sharp=max(sharp,fwidth(length(tex2Dlod(s0,float4(x,0,4)).xyz)))*pow(2,Balance/2.);
    sharp=max(sharp,fwidth(length(tex2Dlod(s0,float4(x,0,3)).xyz)))*pow(2,Balance/3.);
    sharp=max(sharp,fwidth(length(tex2Dlod(s0,float4(x,0,2)).xyz)))*pow(2,Balance/4.);
    c=sharp*4;
    c=saturate(1-pow(sharp*SharpEdges,.5));
    c=tex2Dlod(s0,float4(x,0,1+saturate(Width)*.6*pow(c.x,1./(1+max(0,Width-1)))*log2(max(R.x,R.y))));
    c.a=1;
    return c;
}

void vs2d(inout float4 vp:POSITION0){vp.xy*=2;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
