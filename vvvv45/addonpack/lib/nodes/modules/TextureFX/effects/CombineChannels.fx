float2 R;
float4 faderA;
float4 faderB;

texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 pAdd(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 a=tex2D(s0,x);
    float4 b=tex2D(s1,x);
    float4 c=a*faderA+b*faderB;
    return c;
}
float4 pMul(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 a=tex2D(s0,x);
    float4 b=tex2D(s1,x);
    float4 c=lerp(1,a,faderA)*lerp(1,b,faderB);
    return c;
}
float4 pMax(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 a=tex2D(s0,x);
    float4 b=tex2D(s1,x);
    float4 c=lerp(a,b,a*faderA<b*faderB);
    return c;
}
float4 pMin(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 a=tex2D(s0,x);
    float4 b=tex2D(s1,x);
    float4 c=lerp(a,b,a*faderA>b*faderB);
    return c;
}
void vs2d(inout float4 vp:POSITION0){vp.xy*=2;}
technique Add{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pAdd();}}
technique Mul{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pMul();}}
technique Max{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pMax();}}
technique Min{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pMin();}}
