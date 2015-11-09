float2 R;
float Brightness =1.0;
float Shape=0.0;
float Radius <float uimin=0.0;float uimax=1.0;> =0.8;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
bool Alpha=0;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
	float lod=log2(max(R.x,R.y));
    float4 c=0;
	float kk=0;
	for(float i=0;i<min(lod-(1-Radius*lod),14);i++){
		float4 nc=tex2Dlod(s0,float4(x,0,1+i));
		float k=pow(2,i*Shape-lod+1)*saturate(Radius*lod-i+1);
		c+=nc*k;
		kk+=k;
	}
	c/=kk;
	c.rgb*=Brightness;
	if(!Alpha)c.a=tex2D(s0,x).a;
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
