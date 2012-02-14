float2 R;
float Depth=1;
float Radius=1;
float Gamma;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float mx(float3 x){return max(x.x,max(x.y,x.z));}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
	float maxlod=log2(max(R.x,R.y));
	float kk=0;
	for (float i=1;i<min(maxlod+1,14);i++){
		float2 off=pow(2,i)/max(R.x,R.y);
		off*=Radius;
		//off*=R/R.x;
		float dx=(mx(tex2Dlod(s0,float4(x-off*float2(1,0),0,i)))-mx(tex2Dlod(s0,float4(x+off*float2(1,0),0,i))))/pow(2,i*Gamma);
		float dy=(mx(tex2Dlod(s0,float4(x-off*float2(0,1),0,i)))-mx(tex2Dlod(s0,float4(x+off*float2(0,1),0,i))))/pow(2,i*Gamma);
		c.xy+=float2(dx,dy);
		c.z+=length(float2(dx,dy));
		kk+=1./pow(2,i*Gamma);
    }
	c.xy/=pow(c.z*.1,.3)/.1;
	float dd=Depth/pow(kk,.7)*8;
	c.z=pow(saturate(1-dd*length(c.xy)),.5);
	c.xy=c.xy*dd+.5;
	
	//c.rgb=c.a;
	c.a=tex2D(s0,x).a;
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
