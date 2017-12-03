float2 R;
float4 ColorA:COLOR = {0.0, 0.0, 0.0, 1};
float4 ColorB:COLOR = {1.0, 1.0, 1.0, 1};
float4 ColorC:COLOR = {0.0, 0.0, 0.0, 1};
bool Grey;
float4 Amount <float uimin=0.0; float uimax=1.0;> = 0.5;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};

float4 p0(float2 x:TEXCOORD0):color{
       x=x+tex2D(s0,x*1.5+.2)*.6;
    float4 c=float4(sin(x.yx*28)+x*x.yx*2,length(x*2),sin(x.x*12+x.y*28))+tex2D(s0,x+.2)*3+tex2D(s0,(x.yx-.45)*.8+.45)*2+tex2D(s0,(x-.5)*998.8+.5)+tex2D(s0,(x.yx-.5)*798.8+.5+.31);
    c=frac(c*sqrt(float4(4.5,5.54,7.5,9)*2243));
    float4 al=pow(c,pow(2,(1-Amount)*18-2)*lerp(3,1,Grey));
    //c=1-step(al,.5);
    if(Grey)al=al.r;
    c=lerp(ColorC,lerp(ColorA,ColorB,saturate(al*2-1)),1-step(al,.5));
    //c.rgb=1-step(al,.5);
    //c.rgb=c.r+c.g+c.b;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Pixels{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
