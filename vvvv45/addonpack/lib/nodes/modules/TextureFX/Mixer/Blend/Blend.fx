float2 R;
float Opacity <float uimin=0.0; float uimax=1.0;> = 1.0;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
#define bld(op,c0,c1) float4(lerp((c0*c0.a+c1*c1.a*(1-c0.a))/saturate(c0.a+c1.a*(1-c0.a)),(op),c0.a*c1.a).rgb,saturate(c0.a+c1.a*(1-c0.a)))

float4 pNORMAL(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(c1,c1,c0);
    return c;
}
float4 pADD(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(c0+c1,c0,c1);
    return c;
}
float4 pSUBTRACT(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(c0-c1,c0,c1);
    return c;
}
float4 pSCREEN(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(c0+c1*saturate(1-c0),c0,c1);
    return c;
}
float4 pMUL(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(c0*c1,c0,c1);
    return c;
}
float4 pDARKEN(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(min(c0,c1),c0,c1);
    return c;
}
float4 pLIGHTEN(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(max(c0,c1),c0,c1);
    return c;
}
float4 pDIFFERENCE(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(abs(c0-c1),c0,c1);
    return c;
}
float4 pEXCLUSION(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(c0+c1-2*c0*c1,c0,c1);
    return c;
}
float4 pOVERLAY(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld((c0<.5)?(2*c0*c1):1-2*(1-c0)*(1-c1),c0,c1);
    return c;
}
float4 pHARDLIGHT(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld((c1<.5)?(2*c0*c1):1-2*(1-c0)*(1-c1),c0,c1);
    return c;
}
float4 pSOFTLIGHT(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(2*c0*c1+c0*c0-2*c0*c0*c1,c0,c1);
    return c;
}
float4 pDODGE(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld((c1==1)?1:c0/(1-c1),c0,c1);
    return c;
}
float4 pBURN(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld((c1==0)?0:1-(1-c0)/c1,c0,c1);
    return c;
}
float4 pREFLECT(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld((c1==1)?1:c0*c0/(1-c1),c0,c1);
    return c;
}
float4 pGLOW(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld((c0==1)?1:c1*c1/(1-c0),c0,c1);
    return c;
}
float4 pFREEZE(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld((c1==0)?0:1-pow(1-c0,2)/c1,c0,c1);
    return c;
}
float4 pHEAT(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld((c0==0)?0:1-pow(1-c1,2)/c0,c0,c1);
    return c;
}
float4 pDIVIDE(float2 x:TEXCOORD0):color{float4 c0=tex2D(s0,x);float4 c1=tex2D(s1,x)*float4(1,1,1,Opacity);
    float4 c=bld(c0/c1,c0,c1);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Normal{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pNORMAL();}}
technique Screen{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pSCREEN();}}
technique Multiply{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pMUL();}}
technique Add{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pADD();}}
technique Subtract{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pSUBTRACT();}}
technique Darken{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pDARKEN();}}
technique Lighten{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pLIGHTEN();}}
technique Difference{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pDIFFERENCE();}}
technique Exclusion{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pEXCLUSION();}}
technique Overlay{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pOVERLAY();}}
technique Hardlight{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pHARDLIGHT();}}
technique Softlight{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pSOFTLIGHT();}}
technique Dodge{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pDODGE();}}
technique Burn{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pBURN();}}
technique Reflect{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pREFLECT();}}
technique Glow{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pGLOW();}}
technique Freeze{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pFREEZE();}}
technique Heat{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pHEAT();}}
technique Divide{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pDIVIDE();}}