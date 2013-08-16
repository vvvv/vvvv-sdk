float2 R;
float2 G;
float tm;
float Balance <float uimin=-5.0;> =1.0;
bool AntiAlias=0;
float Scale <float uimin=0;> =1.0;
float Seed;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;};



float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float mx(float3 x){return max(x.x,max(x.y,x.z));}
float4 p0(float2 vp:vpos,float2 uv:TEXCOORD0,float4 vc:COLOR0):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
	c=tex2D(s0,vc.xy);
	//c.a=tex2D(s1,vc.xy).a;
	float cr=length(uv-.5);
	c.a=smoothstep(.50001,.5-AntiAlias*fwidth(cr),cr)*c.a*saturate(vc.a);
	//c.a=(cr<.5)*(vc.a>.1);
	
    return c;
}

void v0(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0, out float ps:PSIZE, out float4 vc:COLOR0)
{	
	float2 p=vp.xy;
	//vp.xy=vp.xy/128.;
	vp.xy=vp.xy/G+float2((vp.y%2),0)/G*0.5;
	float ln=saturate(1-(p.y+p.x/G.x)/G.y);
	//ln=1-frac(ln*2.1);
	vc=tex2Dlod(s0,float4(vp.xy*.5+.5,0,0));
	uv+=.5/R;
	for (float i=0;i<4;i++)
	{
		float2 sz=pow(2,i*5+3);
		vp.xy+=sin((vp.yx)*sz+i*8+ln*2+Seed/sz)*58/(sz);
	}
	vp.xy=(frac(vp.xy)*2-1);
	vc=tex2Dlod(s0,float4(vp.xy*.5+.5,0,0));

	vc.xy=vp.xy*.5+.5;
	//ps=(p.x/G.x+p.y);
	//ps=8*pow(3.6,saturate(mx(vc)+.0*pow(sin(ln*11444),13)));
	//ps=16*pow(2,sin(mx(sh)*18)*2-.5);
	//ps*=.153/pow(.1+length(sh.xyz)*.063,1.59);
	//ps=12;
	//ps=pow(saturate(1-ln)*1.01,pow(2,2))*3;
	//ps=pow(2,(pow(saturate(1-ln)*.98,pow(2,max(0,Balance)))-.51)*23)*pow(2,min(0,Balance));
	//ps=pow(ln,2)*110;
	float bal=Balance;
	ps=pow(ln,pow(2,bal))*(sqrt(R.x*R.y)/pow(2,5))*pow(2,bal*.7)*1;
	//ps=6;
	ps=min(ps,2048)*Scale;
	//if(Balance<1)ps=8;
	vc.a=(ps);
	//if(vc.a<.9)ps=0;
	vp.y*=-1;
}
technique DotPainter{pass pp0{FillMode=Point;PointSpriteEnable=TRUE;AlphaBlendEnable=TRUE;SrcBlend=SRCALPHA;DestBlend=INVSRCALPHA;vertexshader=compile vs_3_0 v0();pixelshader=compile ps_3_0 p0();}}

