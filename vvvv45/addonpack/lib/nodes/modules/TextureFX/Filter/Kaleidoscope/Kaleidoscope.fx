float2 R;
float2 Scale;
float OffsetX <float uimin=-1.0; float uimax=1.0;> = 0.0;
float OffsetY <float uimin=-1.0; float uimax=1.0;> = 0.0;
float Rotate;
float MirrorAngle;
int Divisions <float uimin=0;> =5;
bool Aspect;
bool Filter;

texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;};

float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}

float4 p0(float2 x:TEXCOORD0):color{float2 asp=lerp(1,R.x/R,Aspect);
    float2 vp=x*R-.25;
    //float2 dx=r2d((x-.5-float2(OffsetX,OffsetY))/asp,Rotate)/Scale*asp+.5;
	
	float2 dx=(x-.5)/asp;
	dx=r2d(dx,MirrorAngle);
	float ang=atan2(dx.x,dx.y)/acos(-1)/2+.5;
	ang=(ang+.5/Divisions);
	float rad=length(dx);
	float fng=floor(ang*Divisions)/Divisions;
	
	dx=r2d(dx,-floor(ang*Divisions)/Divisions);
	dx.x=abs(dx.x);
	dx=r2d(dx,-MirrorAngle);
	//dx=r2d(dx,-fng-((floor(ang*Divisions*2)%2)*frac(ang*Divisions*2)));
	dx=dx*asp+.5;
	if(Divisions==0)dx=x;
	dx=r2d((dx-.5-float2(OffsetX,OffsetY))/asp,Rotate)/Scale*asp+.5;
    float4 c=tex2D(s1,dx);
	if(Filter)c=tex2D(s0,dx);
	//c.xyz=abs(frac(ang*Divisions)-.5)*2;
	//c.x=floor(ang*13*2)%2;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;AddressU[1]=CLAMP;AddressV[1]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;AddressU[1]=WRAP;AddressV[1]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;AddressU[1]=MIRROR;AddressV[1]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;AddressU[1]=BORDER;AddressV[1]=BORDER;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
