float2 R;

int Divisions <float uimin=1;> =5;
int Iterations <float uimin=1;float uimax=70;> =2;
float Rotate;
float Zoom =0;
float2 Center <float uimin=-1.0; float uimax=1.0;> =0.0;
float IterationZoom =0;

float2 CellOffset=0;
float CellRotate =0;
float2 CellScale =1;

float4 ControlFactor={1,0,0,0};

bool Aspect;
bool Filter;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;};
texture texMASK;
sampler sMASK=sampler_state{Texture=(texMASK);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float2 kal(float2 x,float2 sz){
	float2 dx=(x-.5);
	float an=atan2(dx.x,-dx.y)/acos(-1)/2+.5;	
	float2 xx=r2d(dx,floor(an*sz+.5)/sz);
	xx.x=abs(xx.x);
	xx+=.5;
	return xx;
}

float4 p0(float2 x:TEXCOORD0,float2 vp:VPOS):color{float2 asp=lerp(1,R/R.x,Aspect);

	//R=512;
   // float2 vp=x*R-.25;
	x=(vp+.5)/R;
	//float2 asp=R/R.x;
	float4 mask=tex2Dlod(sMASK,float4(x,0,1));
	float cr=CellRotate+ControlFactor.x*3*mask;
	float2 co=CellOffset+mask.xy*ControlFactor.zw;
	
	float sz=Divisions;
	float zz=pow(2,Zoom*5-1);
	zz*=pow(2,ControlFactor.y*(tex2Dlod(sMASK,float4(x,0,1))));
	float2 Off=Center;
	float2 dx=r2d((x-.5+Off)*asp,Rotate)*zz+.5;
	float2 xx=kal(dx,sz);
	for(float i=0;i<min(Iterations-1,90);i++){
		xx*=pow(2,IterationZoom*0.1);
    	if(xx.y>1)xx=kal(float2(xx.x,2-xx.y),sz);	
	}
	//xx=(xx-.5)/asp+.5;
	xx=r2d(xx-.5,cr-Rotate)/asp+.5;
	xx+=co*2-1;
	xx=(xx-.5)*CellScale+.5;
	float4 c=tex2D(s1,xx);
	if(Filter)c=tex2D(s0,xx);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;AddressU[1]=CLAMP;AddressV[1]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;AddressU[1]=WRAP;AddressV[1]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;AddressU[1]=MIRROR;AddressV[1]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;AddressU[1]=BORDER;AddressV[1]=BORDER;BorderColor[1]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
