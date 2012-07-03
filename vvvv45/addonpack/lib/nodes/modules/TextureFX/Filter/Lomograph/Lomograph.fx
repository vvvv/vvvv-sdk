float2 R;
float Start <float uimin=0.0;float uimax=1.0;> =1.0;
float Amount <float uimin=0.0;float uimax=1.0;> =1.0;
float Blur <float uimin=0.0;float uimax=1.0;> =1.0;
float Noise <float uimin=0.0;float uimax=1.0;> =1.0;
float Dodge <float uimin=0.0;float uimax=1.0;> =1.0;
float Color <float uimin=0.0;float uimax=1.0;> =0.5;
float Contrast <float uimin=0.0;float uimax=1.0;> =1.0;
float Level <float uimin=0.0;float uimax=1.0;> =1.0;
float Effect <float uimin=0.0;float uimax=1.0;> =1.0;
float Flare <float uimin=0.0;float uimax=1.0;> =1.0;
int Type <float uimin=0.0;float uimax=7.0;> =0;
int Iterations <float uimin=1.0;float uimax=20.0;> =4;

texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=WRAP;};
float3 EffectShape1(float3 x,float effect){
	float4 B=float4(0.49,-1.04,1.24,0.24);
	float4 G=float4(-0.64,0.56,0.98,0.09);
	float4 R=float4(-0.42,0.83,0.5,0.09);
	x.r=lerp(x,x*x*x*R.x+x*x*R.y+x*R.z+R.w,effect).r;
	x.g=lerp(x,x*x*x*G.x+x*x*G.y+x*G.z+G.w,effect).g;
	x.b=lerp(x,max(.24,x*x*x*B.x+x*x*B.y+x*B.z+B.w),effect).b;
	return x;
}
float3 EffectShape2(float3 x,float effect){
	float4 B=float4(0.1,-0.38,0.98,0.23);
	float4 G=float4(-1.47,1.92,0.44,0.11);
	float4 R=float4(-1.17,2.46,-0.1,0.11);
	effect*=4./5.;
	x.r=lerp(x,x*x*x*R.x+x*x*R.y+x*R.z+R.w,effect).r;
	x.g=lerp(x,x*x*x*G.x+x*x*G.y+x*G.z+G.w,effect).g;
	x.b=lerp(x,x*x*x*B.x+x*x*B.y+x*B.z+B.w,effect).b;
	return x;
}
float3 EffectShape3(float3 x,float effect){
	float4 B=float4(-0.84,0.94,0.6,0.04);
	float4 G=float4(-0.84,1.14,0.6,0.0);
	float4 R=float4(4.07,-1.31,0.0,-1.76);
	effect*=4./5.;
	x.r=lerp(x,x*x*x*R.x+x*x*R.y+x*R.z+R.w*x*x*x*x*x,effect).r;
	x.g=lerp(x,x*x*x*G.x+x*x*G.y+x*G.z+G.w,effect).g;
	x.b=lerp(x,x*x*x*B.x+x*x*B.y+x*B.z+B.w*x*x*x*x*x,effect).b;
	return x;
}
float3 EffectShape4(float3 x,float effect){
	float4 B=float4(0.0,0.0,0.81,0.09);
	float4 G=float4(-1.76,2.58,0.17,0.0);
	float4 R=float4(-0.53,2.24,-0.21,0.0);
	effect*=4./5.;
	x.r=lerp(x,min(1.3-x*.3,x*x*x*R.x+x*x*R.y+x*R.z+R.w),effect).r;
	x.g=lerp(x,x*x*x*G.x+x*x*G.y+x*G.z+G.w,effect).g;
	x.b=lerp(x,max(.09,x*x*x*B.x+x*x*B.y+x*B.z+B.w),effect).b;
	return x;
}
float3 EffectShape5(float3 x,float effect){
	float4 B=float4(0.0,0.0,1.5,-0.28);
	float4 G=float4(0.01,0.0,1.03,-0.02);
	float4 R=float4(0.0,0.0,0.77,0.1);
	effect*=4./5.;
	x.r=lerp(x,x*x*x*R.x+x*x*R.y+x*R.z+R.w,effect).r;
	x.g=lerp(x,x*x*x*G.x+x*x*G.y+x*G.z+G.w,effect).g;
	x.b=lerp(x,max(-.3*x,x*x*x*B.x+x*x*B.y+x*B.z+B.w),effect).b;
	return x;
}
float3 EffectShapeSEPIA(float3 x,float effect){
	float4 B=float4(-0.31,0.33,0.8,0.11);
	float4 G=float4(-2.53,3.0,0.03,0.02);
	float4 R=float4(-1.24,2.08,.14,.11);
	x.r=lerp(x,min(.66+x*.32,x*x*x*R.x+x*x*R.y+x*R.z+R.w),effect).r;
	x.g=lerp(x,min(.43+x*.53,x*x*x*G.x+x*x*G.y+x*G.z+G.w+x*x*x*x*x*.47),effect).g;
	x.b=max(.1*effect,lerp(x,x*x*x*B.x+x*x*B.y+x*B.z+B.w,effect).b);
	return x;
}
float3 EffectShapeE6C41(float3 x,float effect){
	x=lerp(lerp(x,max(x.x,max(x.y,x.z)),.1),min(x.x,min(x.y,x.z)),.1);
	float3 y=x;
	float4 B=float4(-0.18,0.96,-0.96,1.0);
	float4 G=float4(0.55,-1.83,-0.61,2.77);
	float4 R=float4(0.23,-0.74,0.52,-0.01);
	x.g=lerp(x,x*.11+x*x*G.w+x*x*x*G.z+x*x*x*x*G.y+x*x*x*x*x*x*x*G.x,effect).g;
	x.r=lerp(x,tanh(x*5-2.9)/2+.5+x*R.w+x*x*R.z+x*x*x*R.y+x*x*x*x*x*x*R.x,effect).r;
	x.b=lerp(x,tanh(x*13-2.2)/10+.09+x*B.w+x*x*B.z+x*x*x*B.y+x*x*x*x*x*x*B.x,effect).b;
	return x;
}
float3 EffectShapeBW(float3 x,float effect){
	x=dot(x.xyz,normalize(float3(.36,.36,.28))/sqrt(3));
	return x;
}
float3 Vignette(float3 x,float glow,float amount,float start,float dodge){
	start*=.6;
	float Sglow=max(0,glow-start)/(1-start);
	float Dglow=start-glow;
	x=x*(1-(1.3-pow(x,2))*pow(Sglow*2,1.5)*.6*amount);
	x*=1+pow(max(0,Dglow)*2,2)*(x*5-x*x*10+5*x*x*x)*2*dodge*pow(1.34-start,2);
	return x;
}
float3 ColorLevel(float3 x,float level){
	float4 k0=float4(0.27,0.11,0.455,-0.006);
	float4 k1=float4(-0.6,-0.13,1.69,0.00);
	x=lerp(min(x*x*x*k0.x+x*x*k0.y+x*k0.z+k0.w,.84),x,saturate(level*2)); //.84 = .83
	x=lerp(x,min(x*x*x*k1.x+x*x*k1.y+x*k1.z+k1.w+x*x*x*x*x*.07,1.02),saturate(level*2-1));
	return x;
}
float3 ColorShape(float3 x,float color){
	x+=(color-.5)*float3(.0,.08,-.24);
	return x;
}
float3 ColorContrast(float3 x,float contrast){
	float4 k0=float4(-1.24,1.84,0.4,0.0);
	x=lerp(x,x*x*x*k0.x+x*x*k0.y+x*k0.z+k0.w,contrast*(1+(.6-pow(abs(x-.5),.5))));
	return x;
}
float3 sat(float3 c){
	//c.rgb*=float2(.06,1).yxx;
	c.rgb+=dot(c.rgb,.3);
	return c;
}
float vstep(float a,float b,float x){
	x=saturate((x-a)/(b-a));
	x+=smoothstep(.8,1,x)*.2;
	return x;
}
/*
float3 AddFlare(float2 x){
	float2 sx=float2(5,.47);
	float3 v=0;
	float yR=(floor(x.y*sx.x)/sx.x);
	float yL=(floor(x.y*sx.y)/sx.y);
	
	float4 c0=tex2Dlod(s0,float4(.97,(floor(x.y*sx.x)/sx.x),0,4.5));
	float4 c1=tex2Dlod(s0,float4(.97,(floor(x.y*sx.x+1)/sx.x),0,4.5));
	float4 c2=tex2Dlod(s0,float4(.02,(floor(x.y*sx.y)/sx.y),0,4.5));
	float4 c3=tex2Dlod(s0,float4(.02,(floor(x.y*sx.y+1)/sx.y),0,4.5));
	float4 mc=c0;
	if(length(c2));
	v+=pow(vstep(0,1,x.x),2+0*tex2Dlod(s0,float4(.7,x.y,0,1)))*c0*smoothstep(1,0,frac(x.y*sx.x));
	v+=pow(vstep(0,1,x.x),2+0*tex2Dlod(s0,float4(.7,x.y,0,1)))*c1*smoothstep(0,1,frac(x.y*sx.x));
	v+=pow(vstep(1,0,x.x),2+0*tex2Dlod(s0,float4(.2,x.y,0,1)))*c2*smoothstep(1,0,frac(x.y*sx.y));
	v+=pow(vstep(1,0,x.x),2+0*tex2Dlod(s0,float4(.2,x.y,0,1)))*c3*smoothstep(0,1,frac(x.y*sx.y));

	return v;
}
//*/
/*
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);

	float3 z=c.rgb;
	
	float itr=min(Iterations,10);
	float3 v=AddFlare(x)*Flare/itr;
	
	//v*=.0;
	for(float i=0;i<itr;i++){
	z=ColorShape(z,Color);
	z=Vignette(z,length((x-.5)*R/R.x),Amount,Start,Dodge);	
	z=ColorContrast(z,Contrast);
	z=ColorLevel(z,Level);
	z+=v/(.7+z);

	switch(Type%8){
		case 0 : z=EffectShape1(z,Effect);break;
		case 1 : z=EffectShape2(z,Effect);break;
		case 2 : z=EffectShape3(z,Effect);break;
		case 3 : z=EffectShape4(z,Effect);break;
		case 4 : z=EffectShape5(z,Effect);break;
		case 5 : z=EffectShapeE6C41(z,Effect);break;
		case 6 : z=EffectShapeBW(z,Effect);break;
		case 7 : z=EffectShapeSEPIA(z,Effect);break;
	}
		//z=max(z,0.03);
	//z=EffectShapeSEPIA(z,Effect);
	}
	//z=AddFlare(x);
	//z=EffectShape3(z,Effect);
	c.rgb=z;
	//c.rgb=abs(z-(1-x.y))<.001;
    return c;
}//*/
float4 prepass(float2 x){
	float4 c=tex2Dlod(s0,float4(x,0,1));
	float3 z=c.rgb;
	float itr=min(Iterations,20);
	//float3 v=AddFlare(x)*Flare/itr;
	for(float i=0;i<itr;i++){
	z=ColorShape(z,Color);
	z=Vignette(z,length((x-.5)*R/R.x),Amount,Start,Dodge);	
	z=ColorContrast(z,Contrast);
	z=ColorLevel(z,Level);
	//z+=v/(.7+z);
		
	}
	c.rgb=z;
	return c;
}
float4 p1(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=prepass(x);
	//z=max(z,0.03);
	//c.rgb=EffectShape1(c.rgb,Effect);
	switch(Type%8){
		case 0 : c.rgb=EffectShape1(c.rgb,Effect);break;
		case 1 : c.rgb=EffectShape2(c.rgb,Effect);break;
		case 2 : c.rgb=EffectShape3(c.rgb,Effect);break;
		case 3 : c.rgb=EffectShape4(c.rgb,Effect);break;
		case 4 : c.rgb=EffectShape5(c.rgb,Effect);break;
		case 5 : c.rgb=EffectShapeE6C41(c.rgb,Effect);break;
		case 6 : c.rgb=EffectShapeBW(c.rgb,Effect);break;
		case 7 : c.rgb=EffectShapeSEPIA(c.rgb,Effect);break;
	}
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Lomograph{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}}
