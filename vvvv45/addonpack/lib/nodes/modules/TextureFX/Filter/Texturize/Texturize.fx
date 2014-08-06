float2 R;
float2 G=4;
float Rotate <float uimin=0.0;> =1;
float Zoom <float uimin=0.0;> =1;
float2 Offset <float uimin=0.0;float uimax=1.0;> =1;
bool Aspect=1;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
texture texNOI;
sampler sNOI=sampler_state{Texture=(texNOI);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float sstep(float a,float b,float x){
	return saturate((x-a)/(b-a));
}
float4 slice(float2 x,float2 n,float2 p){
	n-=p;
	float4 noi=tex2D(sNOI,(n+.5)/G);
	float zoom=pow(noi.x,pow(2,Zoom-2))*.5;
	float2 dx=(x-.5+p)*zoom/G;
	float2 asp=R/R.x;
	if(!Aspect)asp=1;
	dx=r2d(dx*asp,(noi.y-.5)*Rotate)/asp;
	dx+=(noi.zw-.5)*(1-zoom)*(G-1)/G*Offset;
	float4 c=tex2Dlod(s0,float4(dx+.5,0,1));
	return c;
}

float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
	float2 tx=frac(x*G);
	float2 fx=floor(x*G);
	c+=slice(tx,fx,float2( 0, 0))*sstep(1,0,abs(tx.x-.5))*sstep(1,0,abs(tx.y-.5));
	c+=slice(tx,fx,float2(-1, 0))*sstep(.5,1.5,tx.x)*sstep(1,0,abs(tx.y-.5));
	c+=slice(tx,fx,float2(+1, 0))*sstep(.5,-.5,tx.x)*sstep(1,0,abs(tx.y-.5));
	c+=slice(tx,fx,float2( 0,-1))*sstep(1,0,abs(tx.x-.5))*sstep(.5,1.5,tx.y);
	c+=slice(tx,fx,float2(-1,-1))*sstep(.5,1.5,tx.x)*sstep(.5,1.5,tx.y);
	c+=slice(tx,fx,float2(+1,-1))*sstep(.5,-.5,tx.x)*sstep(.5,1.5,tx.y);
	c+=slice(tx,fx,float2( 0,+1))*sstep(1,0,abs(tx.x-.5))*sstep(.5,-.5,tx.y);
	c+=slice(tx,fx,float2(-1,+1))*sstep(.5,1.5,tx.x)*sstep(.5,-.5,tx.y);
	c+=slice(tx,fx,float2(+1,+1))*sstep(.5,-.5,tx.x)*sstep(.5,-.5,tx.y);
	c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Texturize{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
