float2 R;
float4 tgt:COLOR;
texture tex0,tex1,tex2;
sampler s0=sampler_state{Texture=(tex0);AddressU=WRAP;AddressV=WRAP;MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);AddressU=WRAP;AddressV=WRAP;MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
#define PW (15.)
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=pow(tex2D(s0,x),PW);
    return c;
}
float4 p1(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=pow(tex2D(s0,x),1./PW);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Average{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}}
