float2 R;
#include "ColorSpace.fxh"
float2 r2d(float2 x,float a){return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float4 pHSL(float2 x:TEXCOORD0):color{
       float4 c=1;
       c.rgb=HSVtoRGB(float3(x.x,1,saturate(2-2*x.y)));
       c.rgb=lerp(c.rgb,max(c.r,max(c.g,c.b)),saturate(1-x.y*2));
       return c;
}
float4 pHSV(float2 x:TEXCOORD0):color{
       float4 c=1;
       c.rgb=HSVtoRGB(float3(x.x,1,1-x.y));
       return c;
}
float4 pRADIAL(float2 x:TEXCOORD0):color{
       float4 c=1;
       c.rgb=float3(x.x,x.y,length(x));
       c.rgb=HSLtoRGB(float3(atan2(x.y-.5,x.x-.5)/acos(-1)/2,1,length(x-.5)));
       return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique XY_HSV{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pHSV();}}
technique XY_HSL{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pHSL();}}
technique RADIAL_HSV{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pRADIAL();}}
