float2 R;
float2 r2d(float2 x,float a){return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float Count;
bool Aspect=true;
float pw=1;
float4 ColorA:COLOR = {0.0, 0.0, 0.0, 1};
float4 ColorB:COLOR = {1.0, 1.0, 1.0, 1};
bool Field=1;
texture tData;
sampler sD=sampler_state{Texture=(tData);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;};
float4 f0(float2 vp:vpos):color{float2 x=(vp+.5)/R;float2 asp=lerp(1,min(R.x,R.y)/R,Aspect);
	float2 dx=(x-.5)*2;
	float f2;
	for (float i=0;i<min(Count,128);i++){
		float4 data=tex2Dlod(sD,float4((i+.5)/64,0.5,0,1));
		data.xy=data.xy;
        data.w=max(.0001,data.w);
        //dx=(dx-data.xy)/asp;dx=zom(dx,data.z*pow(data.w-length(dx),.5)*smoothstep(1,0,length(dx)/data.w))*asp+data.xy;
		float nc=length((dx/asp-data.xy))*8;
		f2+=pow(1./nc*data.z,pw);
    }
	f2=pow(1./f2,1./pw);
	return f2;
}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;float2 asp=lerp(1,min(R.x,R.y)/R,Aspect);
	float2 dx=(x-.5)*2;
	float4 c=0;
	float f2=f0(vp);
	c=smoothstep(0.9999-fwidth(f2),1.0,f2);
	if(Field)c=f2;
	c=lerp(ColorB,ColorA,saturate(c));
	return c;
}


void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Metaballs{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}

