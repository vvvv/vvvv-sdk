float2 R;
float2 FromXY;
float2 ToXY;
float2 Ramp;
float Grayscale;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
#define ramp(FromXY,ToXY,s1,c) float4(tex2D(s1,lerp(FromXY,ToXY,c.r)).r,tex2D(s1,lerp(FromXY,ToXY,c.g)).g,tex2D(s1,lerp(FromXY,ToXY,c.b)).b,tex2D(s1,lerp(FromXY,ToXY,c.a)).a)
float4 pCOLOR(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);float pa=c.a;
    c=ramp(FromXY,ToXY,s1,c);
    c.a*=pa;
    return c;
}
float4 pLUMA(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);float pa=c.a;
    c.rgb=dot(c.rgb,float3(.33,.59,.11));
    c=ramp(FromXY,ToXY,s1,c);
    c.a*=pa;
    return c;
}
float3 rgb2hsl(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=(cmax+cmin)/2,d=(cmax-cmin),s=l>.5?d/(2-cmax-cmin)/l/2:d/(cmax+cmin),h;float3 ch=(cmax==c);if(ch.r==ch.g&&ch.r==1)ch.rg=float2(1,0);if(ch.g==ch.b&&ch.g==1)ch.gb=float2(1,0);if(ch.b==ch.r&&ch.b==1)ch.br=float2(1,0);h=frac((dot(min(2,ch),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}
float4 pHUE(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);float pa=c.a;
    c.rgb=rgb2hsl(c.rgb).x;
    c=ramp(FromXY,ToXY,s1,c);
    c.a*=pa;
    return c;
}
float4 pSATURATION(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);float pa=c.a;
    c.rgb=rgb2hsl(c.rgb).y;
    c=ramp(FromXY,ToXY,s1,c);
    c.a*=pa;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique ColorRamp{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pCOLOR();}}
technique LumaRamp{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pLUMA();}}
technique HueRamp{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pHUE();}}
technique SaturationRamp{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pSATURATION();}}
