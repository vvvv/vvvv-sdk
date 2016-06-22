float2 R;
float Tolerance <float uimin=0.0; float uimax=1.0;> = 0.1;
float Softness <float uimin=0.0; float uimax=1.0;> = 0;
float4 tgt:COLOR;
texture tex0,tex1,tex2;
sampler s0=sampler_state{Texture=(tex0);AddressU=MIRROR;AddressV=MIRROR;MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);AddressU=MIRROR;AddressV=MIRROR;MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);AddressU=MIRROR;AddressV=MIRROR;MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    //float key=dot(c.rgb,1)/3.>Threshold;
    float key=smoothstep(Tolerance+Softness+.001,Tolerance,distance(c.rgb,tgt.rgb));

    c.xy=float2(x.x,1-x.y)*key;
    c.z=key;
    c.w=1;
    return c;
}
float4 p1(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2Dlod(s0,float4(x,0,88));
    c.xy=c.xy/c.z;
    return c;
}
float4 p2(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float4 k=tex2Dlod(s2,float4(x,0,2));
    float4 p=tex2D(s1,x);
    c.rgb=lerp(lerp(c.rgb,tgt.rgb,k.z),1-tgt.rgb,pow(saturate(1-2*abs(k.z-.5)),.9));
    c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Tracker{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();} pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p2();}}
