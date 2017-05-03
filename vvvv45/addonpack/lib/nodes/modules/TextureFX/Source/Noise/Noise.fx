float2 R;
float4 ColorA:COLOR = {0.0, 0.0, 0.0, 1};
float4 ColorB:COLOR = {1.0, 1.0, 1.0, 1};
bool Grey;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float Seed=0;
float4 p0(float2 x:TEXCOORD0):color{
	x=x+tex2D(s0,x*1.5+.2).x*.6;
	x+=sin(x.yx*28+Seed)*4;
    float4 c=float4(sin(x.yx*28)+x*x.yx*2,length(x*2),sin(x.x*12+x.y*28))+tex2D(s0,x+.2).x*3+tex2D(s0,(x.yx-.45)*.8+.45).x*2+tex2D(s0,(x-.5)*998.8+.5).x+tex2D(s0,(x.yx-.5)*798.8+.5+.31).x;
    c=frac(Seed+c*sqrt(float4(3.5,5.54,7.5,9)*2243));
    if(Grey)c=c.r;
    c=lerp(ColorA,ColorB,c);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Noiz{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
