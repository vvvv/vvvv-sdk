float2 R;
float Parameter <float uimin=0.0; float uimax=1.0;> =0;
texture texMAP;
sampler sMAP=sampler_state{Texture=(texMAP);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
texture texFEED;
sampler sFEED=sampler_state{Texture=(texFEED);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
texture texBRUSH;
sampler sBRUSH=sampler_state{Texture=(texBRUSH);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
bool Reset=0;
bool HideBrush=0;
float Speed <float uimin=0.0;> =1;
float Fade <float uimin=0.0;> =0.1;
float MapShape <float uimin=0.0; float uimax=1.0;> =0.75;
float EdgeWidth <float uimin=0.0001;> =2;
float mx(float3 x){return max(x.x,max(x.y,x.z));}


float surf(sampler s,float2 x){
	float4 c=0;
	float2 e=EdgeWidth/R;
	c+= 4*tex2Dlod(s,float4(x+float2( 0, 0)*e,0,1));
	c+=-1*tex2Dlod(s,float4(x+float2( 1, 0)*e,0,1));
	c+=-1*tex2Dlod(s,float4(x+float2(-1, 0)*e,0,1));
	c+=-1*tex2Dlod(s,float4(x+float2( 0, 1)*e,0,1));
	c+=-1*tex2Dlod(s,float4(x+float2( 0,-1)*e,0,1));
	//c=mx(lerp(tex2Dlod(s,float4(x,0,1)),saturate(abs(c)*8./EdgeWidth-.2),MapEdge));
	return smoothstep(.03,1,mx(lerp(tex2Dlod(s,float4(x,0,1)),saturate(abs(c)*8./EdgeWidth-.2),MapShape)));
}
float4 fMASK(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
	float4 pre=tex2Dlod(sFEED,float4(x,0,1));
	float4 bru=tex2Dlod(sBRUSH,float4(x,0,1));
	float wd=surf(sMAP,x);
	if(HideBrush)bru.a*=pow(wd+.0001,.25);
	for(float i=0;i<1;i+=1./24.){
		float2 off=sin((i+float2(.25,0))*acos(-1)*2);
		float2 dx=x+off/R*wd*Speed;
		float4 nc=tex2Dlod(sFEED,float4(dx,0,1));

		c=max(c,nc*pow(1.01,-Fade*Fade));
		
	}
	c=max(c,bru.a*bru);
	if(Reset)c=float4(0,0,0,0);
    return c;
}
float4 pMASK(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=smoothstep(.1,.12,mx(tex2Dlod(sFEED,float4(x,0,1)).rgb));
	c=tex2Dlod(sMAP,float4(x,0,1))*float4(1,1,1,c.a);
    return c;
}
float4 pCOLOR(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2Dlod(sFEED,float4(x,0,1));
	c.a=1;
    return c;
}
float4 fCOLOR(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
	float4 pre=tex2Dlod(sFEED,float4(x,0,1));
	float4 bru=tex2Dlod(sBRUSH,float4(x,0,1));
	float wd=surf(sMAP,x);
	if(HideBrush)bru.a*=pow(wd+.0001,.25);
	float4 mc=0;
	mc=float4(bru.rgb*bru.a,bru.a);
	for(float i=0;i<1;i+=1./24.){
		float2 off=sin((i+float2(.25,0))*acos(-1)*2);
		float2 dx=x+off/R*wd*Speed;
		float4 nc=tex2Dlod(sFEED,float4(dx,0,1));
		if(nc.a>mc.a){mc=lerp(nc,mc,saturate((mc.a-nc.a)*88));}
		//mc=lerp(mc,nc,smoothstep(-1,1,8*(nc.a-mc.a)));
	}
	mc.a*=pow(1.01,-Fade*Fade);
	c=mc;
	c=saturate(c);
	if(Reset)c=float4(0,0,0,0);
    return c;
}
float4 fTEXCOORD(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
	float4 pre=tex2Dlod(sFEED,float4(x,0,1));
	float4 bru=tex2Dlod(sBRUSH,float4(x,0,1));
	float wd=surf(sMAP,x);
	if(HideBrush)bru.a*=pow(wd+.0001,.25);
	c.xy=x;
	for(float i=0;i<1;i+=1./24.){
		float2 off=sin((i+float2(.25,0))*acos(-1)*2);
		float2 dx=x+off/R*wd*Speed;
		float4 nc=tex2Dlod(sFEED,float4(dx,0,1));
		nc.a*=pow(1.01,-Fade*Fade);
		//c=max(c,nc*);
		if(nc.a>c.a){c.xy=nc.xy;c.a=nc.a;}
		
	}
	c=lerp(c,float4(x.xy,0,1),bru.a);
	//c=max(c,bru.a*bru);
	if(Reset)c=float4(0,0,0,0);
    return c;
}
float4 pTEXCOORD(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(sFEED,x);
	//c=tex2Dlod(sMAP,float4(c.xy,0,1))*float4(1,1,1,c.a);

	c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
//technique GrowthMAX{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pMAX();}pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique ColorPaint{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 fCOLOR();}pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pCOLOR();}}
technique GrowthMask{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 fMASK();}pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pMASK();}}
//technique TexCoords{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 fTEXCOORD();}pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pTEXCOORD();}}
