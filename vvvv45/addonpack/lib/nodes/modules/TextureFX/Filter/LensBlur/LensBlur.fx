float2 R;
float Width <float uimin=0.0; float uimax=1.0;> =0.3;
float Gamma =5;
float Seed <float uimin=0.0; float uimax=1.0;> =1;
int Iterations <float uimin=1.0; float uimax=80.0;> =15;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};

float4 pGAMMA(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
	c.rgb=pow(length(c.rgb),Gamma)*normalize(c.rgb);
	//c=pow(c,Gamma);
    return c;
}
float mx(float3 x){return max(x.x,max(x.y,x.z));}
float4 pBLUR(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
	float mask=mx(tex2D(s1,x).rgb);
	//c=tex2Dlod(s0,float4(x,0,7))*12;
	//float wd=pow(Width,4)/pow(pow(2,1./sqrt(mask)),2);
	float wd=pow(Width,2)*mask;
	float iter=12;
	float2 def=length(sin(x*718+Seed*222+wd*888+1882*tex2D(s0,x)))*12.7+1*dot(vp%2,.03*float2(1,2));
	//def=0;
	for(float i=0;i<1;i+=1./max(0,min(Iterations,80))){
		float2 off=pow(i*.9999,.4)*sin((i*iter*8*.315+def+float2(.25,0))*acos(-1)*2);
		//off*=R.x/R;
		c+=float4(tex2Dlod(s0,float4(x+off*wd*R.x/R,0,1+log2(min(R.x,R.y)*wd*.6*pow(1-length(off),.6)))).rgb,1)*pow(2,-3*pow(length(off),2));
	}
	c/=c.a;
	//c=pow(c,1./Gamma);
	c.rgb=pow(length(c.rgb),1./Gamma)*normalize(c.rgb);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique ShaderFilter{
	pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pGAMMA();}
	pass pp1{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pBLUR();}
}
