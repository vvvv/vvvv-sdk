float2 R;
float2 SR;
float ControlFactor <float uimin=0.0; float uimax=1.0;> =0;
float LookupRadius <float uimin=0.0; float uimax=1.0;> =1;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4x4 tWVP:WORLDVIEWPROJECTION;

float4 pPOINTS(float2 vp:vpos,float2 y:TEXCOORD0):color{float2 x=(vp+.5)/R;
    float2 p=(vp)/(R-1);
	float2 u=p;
	float maxcol=0;
	float2 shift=0;
	u+=sin(u.yx*1388*sqrt(float2(5,7)))*.142;
	for(float i=0;i<1;i+=1./52){
		float2 off=sqrt(i+.03)*sin((i*7.7+float2(.25,0))*acos(-1)*2);
		off*=LookupRadius*.5;
		float samp=tex2Dlod(s0,float4(u+off,0,1.5)).a;
		if(samp>maxcol){shift=off;maxcol=samp;}
	}
	//float2 shift2=(frac(shift+u)-u);

	shift=(frac(shift+u)-u);
	u+=shift*ControlFactor;


	return float4(u.xy,1,1);
}
float4 pEDGE(float2 vp:vpos):color{float2 x=(vp+.5)/R;
	float4 c=0;
	for(float i=0;i<1;i+=1./12){
		float2 off=sqrt(i+.02)*sin((i*7+float2(.25,0))*acos(-1)*2);
		c+=float4(tex2D(s0,x+off*.005).xyz,1);
	}
	c.a=pow(dot(c.rgb/c.a-tex2D(s0,x).rgb,1),.5)*2;
	c.rgb=tex2D(s0,x);
    return c;
}
float Blur <float uimin=0.0;float uimax=1.0;> =0.1;
float4 ColorTriangles:COLOR ={1.0,1.0,1.0,1.0};
float4 ColorEdges:COLOR ={0.9,0.9,0.9,1.0};
float4 pTRI(float2 x:TEXCOORD0,float2 t:TEXCOORD1):color{
	float4 c=tex2D(s0,x);
	c*=ColorTriangles;
    return c;
}
float4 pTRIWF(float2 x:TEXCOORD0,float2 t:TEXCOORD1):color{
	float4 c=tex2D(s0,x);
	c*=ColorEdges;
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv=vp;}
void vs3d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.y*=-1;vp=mul(vp,tWVP);}

technique Positions{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pPOINTS();}}
technique Edge{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pEDGE();}}
technique Triangles{pass pp0{AlphaBlendEnable=TRUE;SrcBlend=SRCALPHA;DestBlend=INVSRCALPHA;vertexshader=compile vs_3_0 vs3d();pixelshader=compile ps_3_0 pTRI();}pass pp0{FillMode=WIREFRAME;AlphaBlendEnable=TRUE;SrcBlend=SRCALPHA;DestBlend=INVSRCALPHA;vertexshader=compile vs_3_0 vs3d();pixelshader=compile ps_3_0 pTRIWF();}}


