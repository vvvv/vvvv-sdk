float2 R;
float Amount <float uimin=0.0;float uimax=1.0;> = 1.0;
float Lights <float uimin=0.0;float uimax=1.0;> = 1.0;
float Midtones <float uimin=0.0;float uimax=1.0;> = 1.0;
float Shadows <float uimin=0.0;float uimax=1.0;> = 1.0;
bool Grey;
float Seed=0;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
texture tex1;
sampler s1=sampler_state{Texture=(tex1);MipFilter=POINT;MinFilter=POINT;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};

float4 p0(float2 x:TEXCOORD0):color{
       float2 dx=x+tex2D(s1,x*1.5+.2+Seed).x*.6;
	dx+=sin(dx.yx*28+Seed)*4;
    float4 c=float4(sin(dx.yx*28)+dx*dx.yx*2,length(x*2),sin(dx.x*12+dx.y*28))+tex2D(s1,dx+.2).x*3+tex2D(s1,(dx.yx-.45)*.8+.45).x*2+tex2D(s1,(dx-.5)*998.8+.5).x+tex2D(s1,(dx.yx-.5)*798.8+.5+.31).x;
	c.xyz+=sin(c.yzx*17)*3;
    c=frac(Seed+c*sqrt(float4(4.5,5.54,7.5,9)*2243));
    if(Grey)c=c.r;
    float4 map=tex2D(s0,x);
    c=lerp(map,step(c,pow(map,1)),Amount*(saturate(map*2-1)*Lights+saturate(1-map*2)*Shadows+Midtones*(1-2*abs(frac(map)-.5))));
	c.a=map.a;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Grain{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
