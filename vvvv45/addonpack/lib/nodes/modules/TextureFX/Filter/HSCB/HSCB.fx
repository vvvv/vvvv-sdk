float2 R;
float Hue;
float Saturation;
float Contrast;
float Brightness;
float HueCycles;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
#include "ColorSpace.fxh"
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float3 h=RGBtoHSL(c.rgb);
    h.x=frac(h.x+Hue)*HueCycles;
    h.y=h.y*Saturation;
    c.rgb=HSLtoRGB(h);
    c.rgb=normalize(c.rgb)*sqrt(3)*pow(length(c.rgb)/sqrt(3),pow(2,Contrast))*pow(2,Brightness);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique HSCB{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
