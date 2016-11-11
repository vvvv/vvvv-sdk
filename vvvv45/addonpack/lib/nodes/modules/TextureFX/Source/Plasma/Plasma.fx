float2 R;

float2 Offset=(0,0);
float2 Scale=(1,1);
float Saturation <float uimin=0.0; float uimax=1.0;> = 0.3;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float3 lungth(float2 x,float3 c){
       return float3(length(x+c.r),length(x+c.g),length(x+c.b));
}
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=0;
    x=x*Scale*R/R.x+Offset;
    x+=sin(x.yx*sqrt(float2(13,9)))/5;
    c.rgb=lungth(sin(x*sqrt(float2(33,43))),float3(5,6,7)*Saturation);
    x+=sin(x.yx*sqrt(float2(73,53)))/5;
    c.rgb=2*lungth(sin(x*sqrt(float2(33,23))),c/9);
    x+=sin(x.yx*sqrt(float2(93,73)))/7;
    c.rgb=lungth(sin(x*sqrt(float2(13,1))),c/2);
    c=.5+.5*sin(c*8);
    c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Plasma{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
