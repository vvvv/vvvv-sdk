float2 R;
float4 ColA:COLOR;
float4 ColB:COLOR;
float2 Angle=(0,0);
float2 BumpAmount <float uimin=0.0;> =(1,1);
float2 BumpGamma=(0,0);
float2 Brightness=0;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 q(float2 x,float2 off,float v){return tex2Dlod(s0,float4(x+off/R,0,1+v));}
#define gam(x,y) sign(x)*pow(abs(x),(y))
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float3 hsv2rgb(float3 h){h.z+=max(0,h.y-1);float3 c=saturate((abs(frac(-h.x+float3(3,1,2)/3)*6-3)-1))*h.z;c=lerp(c,max(c.r,max(c.g,c.b)),1-h.y);return c;}
float3 rgb2hsv(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=cmax,d=(cmax-cmin),s=max(abs(c.r-c.g),max(abs(c.g-c.b),abs(c.r-c.b)))/cmax,h;float3 ch=(cmax==c);if(ch.r==ch.g&&ch.r==1)ch.rg=float2(1,0);if(ch.g==ch.b&&ch.g==1)ch.gb=float2(1,0);if(ch.b==ch.r&&ch.b==1)ch.br=float2(1,0);h=frac((dot(min(2,ch),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}
float3 hsl2rgb(float3 h){return lerp(h.z,saturate((abs(frac(-h.x+float3(3,1,2)/3)*6-3)-1))*h.z*2,saturate(h.y)*saturate(2-2*h.z));}
float3 rgb2hsl(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=(cmax+cmin)/2,d=(cmax-cmin),s=l>.5?d/(2-cmax-cmin)/l/2:d/(cmax+cmin),h;float3 ch=(cmax==c);if(ch.r==ch.g&&ch.r==1)ch.rg=float2(1,0);if(ch.g==ch.b&&ch.g==1)ch.gb=float2(1,0);if(ch.b==ch.r&&ch.b==1)ch.br=float2(1,0);h=frac((dot(min(2,ch),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}

float mx(float3 p){return max(p.x,max(p.y,p.z));}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;

    float3 e=float3(1,-1,0);
    
    float4 c=0;
	float2 dir=0;
	float maxlod=log2(max(R.x,R.y));
	for (float i=0;i<11&&i<maxlod;i++)
	{
		float lod=i+1;
		float2 off=pow(2,lod)/R/2;
		float DH=mx(tex2Dlod(s0,float4(x-float2(1,0)*off,0,lod)).xyz)-mx(tex2Dlod(s0,float4(x+float2(1,0)*off,0,lod)).xyz);
		float DV=mx(tex2Dlod(s0,float4(x-float2(0,1)*off,0,lod)).xyz)-mx(tex2Dlod(s0,float4(x+float2(0,1)*off,0,lod)).xyz);
		dir+=float2(DH,DV)*0.5;
		
	}
	float ang=atan2(dir.y,dir.x);
	float rad=length(dir);
	dir=gam(dir,pow(2,BumpGamma*.25*sqrt(smoothstep(0,1,1*abs(dir)))))*BumpAmount/2;

	c+=lerp(ColA*pow(2,Brightness.x),0,r2d(dir,Angle.x).x+.5);
	c+=lerp(ColB*pow(2,Brightness.y),0,r2d(dir,Angle.y+.25).x+.5)*(1-abs(r2d(dir,Angle.x).x));

	c.a=1;
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
