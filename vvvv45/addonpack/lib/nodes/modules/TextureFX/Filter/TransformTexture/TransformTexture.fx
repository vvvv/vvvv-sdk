float2 R;
float BlurWidth <float uimin=0.0;> =1;
int Iterations <float uimin=1.0;float uimax=30.0;> =8;

texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4x4 tTex: TEXTUREMATRIX;
float4x4 tTexP: TEXTUREMATRIX;

float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
	float2 x0=mul(float4(x.xy,0,1),tTex);
	float2 x1=mul(float4(x.xy,0,1),tTexP);
	c=0;
	float itr=Iterations;
	itr=min(30,itr);
	for(int i=0;i<itr&&i<30;i++){
		c+=tex2D(s0,lerp(x0,x1,(i/itr-.5)*BlurWidth+.5))/max(1,itr);
		//c+=.07*x.x;
	}
	
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
