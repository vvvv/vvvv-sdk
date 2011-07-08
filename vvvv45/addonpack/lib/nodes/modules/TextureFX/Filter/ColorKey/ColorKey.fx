float2 R;
float AlphaBlur <float uimin=0.0; float uimax=1.0;> = 0.1;
float4 Col:COLOR;
float sHue <float uimin=0.0; float uimax=1.0;> = 0.1;
float sSaturation <float uimin=0.0; float uimax=1.0;> = 0.1;
float sLightness <float uimin=0.0; float uimax=1.0;> = 01;
float tHue <float uimin=0.0; float uimax=1.0;> = 0.05;
float tSaturation <float uimin=0.0; float uimax=1.0;> = 0.05;
float tLightness <float uimin=0.0; float uimax=1.0;> = 0.05;
bool SourceAlpha;
bool Premultiply;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float3 hsv2rgb(float3 h){h.z+=max(0,h.y-1);float3 c=saturate((abs(frac(-h.x+float3(3,1,2)/3)*6-3)-1))*h.z;c=lerp(c,max(c.r,max(c.g,c.b)),1-h.y);return c;}
float3 rgb2hsv(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=cmax,d=(cmax-cmin),s=max(abs(c.r-c.g),max(abs(c.g-c.b),abs(c.r-c.b)))/cmax,h;float3 ch=(cmax==c);if(ch.r==ch.g&&ch.r==1)ch.rg=float2(1,0);if(ch.g==ch.b&&ch.g==1)ch.gb=float2(1,0);if(ch.b==ch.r&&ch.b==1)ch.br=float2(1,0);h=frac((dot(min(2,ch),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}
float3 hsl2rgb(float3 h){return lerp(h.z,saturate((abs(frac(-h.x+float3(3,1,2)/3)*6-3)-1))*h.z*2,saturate(h.y)*saturate(2-2*h.z));}
float3 rgb2hsl(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=(cmax+cmin)/2,d=(cmax-cmin),s=l>.5?d/(2-cmax-cmin)/l/2:d/(cmax+cmin),h;float3 ch=(cmax==c);if(ch.r==ch.g&&ch.r==1)ch.rg=float2(1,0);if(ch.g==ch.b&&ch.g==1)ch.gb=float2(1,0);if(ch.b==ch.r&&ch.b==1)ch.br=float2(1,0);h=frac((dot(min(2,ch),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}

float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float4 map=tex2Dlod(s0,float4(x,0,(saturate(AlphaBlur)*log2(max(R.x,R.y)))));
    float3 h=rgb2hsl(map.xyz);
    float3 k=rgb2hsl(Col.xyz);
    
    if(!SourceAlpha)c.a=1;
    c.a*=saturate(.5+256/pow(2,sHue*10)*(tHue*.504-min(abs(h.x-k.x),min(abs(h.x-k.x-1),abs(h.x-k.x+1)))));
    c.a*=saturate(.5+256./pow(2,sSaturation*10)*(tSaturation*.504-abs(h.y-k.y)));
    c.a*=saturate(.5+256./pow(2,sLightness*10)*(tLightness*.504-abs(h.z-k.z)));
    if(Premultiply)c.rgb*=sqrt(1./c.a);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique ColorKey{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
