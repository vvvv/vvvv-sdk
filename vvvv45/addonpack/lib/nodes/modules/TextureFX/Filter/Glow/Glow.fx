float2 R;
float PreBlurWidth =0.0;
float GlowAmount <float uimin=0.0;> =0.5;
float GlowShape = 0.1;
float GlowRadius <float uimin=0.0;float uimax=1.0;> =0.8;
float GlowSaturation <float uimin=0.0;> =1.0;
float PostBrightness <float uimin=0.0;> =1.0;
float PreGamma <float uimin=0.0;> =2.0;
float ToneMapPower <float uimin=0.0;float uimax=1.0;> =0.8;
#define PW (4.0)
#define PW0 (7.0)
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
float mx(float3 c){return max(c.x,max(c.y,c.z));}
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}

float4 pPRE(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float lod=1+log2(max(R.x,R.y));
    float4 c=tex2D(s0,x);
	for(float i=0;i<7;i++){
		c+=tex2Dlod(s0,float4(x+sin((i/7.+float2(.25,0))*acos(-1)*2)*PreBlurWidth/R,0,1));
	}
	c/=7;
	//c.rgb=lerp(dot(c.rgb,1./3),c.rgb,GlowSaturation);
	float mc=min(c.r,min(c.g,c.b));
	float gs=GlowSaturation;
	c.rgb=(c.rgb-mc)*gs+lerp(mc,dot(c.rgb,1./3),saturate(1-gs));
	//c.rgb=pow(c.rgb,PreGamma);
	c.rgb=normalize(c.rgb)*pow(length(c.rgb)/sqrt(2),PreGamma)*sqrt(2);
	c.a=pow(length(tex2D(s0,x).rgb),PW0);
    return c;
}

float4 pGLOW(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float lod=log2(max(R.x,R.y));
	float4 s=tex2D(s1,x);
    float4 g=0;
    for(float i=0;i<min(lod-(1-GlowRadius*lod),14);i++){
    	g+=pow(float4(tex2Dlod(s0,float4(x+r2d(vp%4-1.5,i*.25+.125)/R*.5*pow(2,i),0,i+1)).rgb,1)*pow(2,i*GlowShape-lod+1),.8)*saturate(GlowRadius*lod-i+1);
    }
	g.rgb/=g.a;
	//g.rgb/=tex2Dlod(s0,float4(x,0,33)).a;
	float srcAvg=length(tex2Dlod(s1,float4(x,0,33)).rgb);
	float srcMax=pow(tex2Dlod(s0,float4(x,0,33)).a,1./PW0);
	//g.rgb*=srcAvg/srcMax;
	g.rgb*=GlowAmount;
	float4 c=0;
	c.rgb=g;
	//s.rgb=normalize(s.rgb)*sqrt(2)*pow(length(s.rgb)/sqrt(2),PreGamma);
	c.a=pow(length(g.rgb+s.rgb),PW);
    return c;
}
float4 pMIX(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float lod=1+log2(max(R.x,R.y));
    float4 s=tex2D(s1,x);
	float4 g=tex2Dlod(s0,float4(x,0,3));
	//float4 c=tex2D(s0,x);
	for(float i=0;i<5;i++){
		g+=tex2Dlod(s0,float4(x+sin((i/5.+float2(.25,0))*acos(-1)*2)*2/R,0,3));
	}
	g/=6;
	float4 c=s;
	float av=pow(tex2Dlod(s0,float4(x,0,33)).a,1./PW);
	////s=pow(s,PreGamma);
	//s.rgb=normalize(s.rgb)*sqrt(2)*pow(length(s.rgb)/sqrt(2),PreGamma);
	////float mc=min(c.r,min(c.g,c.b));
	////c.rgb=(c.rgb-mc)*pow(GlowSaturation,.3)+lerp(mc,dot(c.rgb,1./3),saturate(1-pow(GlowSaturation,.3)));
	c=pow(s,1+g*2)+g;
	c*=PostBrightness/lerp(1,av+.1,ToneMapPower);
	//c=s+g*Brightness*318;
	
	//c=g+pow(s,1+g);
	//c.rgb=normalize(s.rgb)*pow(length(s.rgb)/sqrt(3),1+g*2)*sqrt(3)+g;
	//c/=1+3*tex2Dlod(s0,float4(x,0,33));
	//c.a=s.a;
	
	c.a=tex2D(s1,x).a;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Glow{
	pass ppPre{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pPRE();}
	pass ppGlo{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pGLOW();}
	pass ppMix{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pMIX();}
}

