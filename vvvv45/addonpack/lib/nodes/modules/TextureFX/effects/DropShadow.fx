float2 R;
float Angle;
float Offset <float uimin=0.0; float uimax=1.0;> = 0.05;
float AlphaBlur <float uimin=0.0; float uimax=1.0;> = 0.1;
float Alpha <float uimin=0.0; float uimax=1.0;> = 0.7;
float Ext;
float4 ShadowColor:COLOR;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
float3 hsv2rgb(float3 h){h.z+=max(0,h.y-1);float3 c=saturate((abs(frac(h.x+float3(1,2,3)/3)*6-3)-1))*h.z;c=lerp(c,max(c.r,max(c.g,c.b)),1-h.y);return c;}
float3 rgb2hsv(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=cmax,d=(cmax-cmin),s=max(abs(c.r-c.g),max(abs(c.g-c.b),abs(c.r-c.b)))/cmax,h=frac((4-dot(min(2,(cmax==c)),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}
float3 hsl2rgb(float3 h){return lerp(h.z,saturate((abs(frac(h.x+float3(1,2,3)/3)*6-3)-1))*h.z*2,saturate(h.y)*saturate(2-2*h.z));}
float3 rgb2hsl(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=(cmax+cmin)/2,d=(cmax-cmin),s=l>.5?d/(2-cmax-cmin)/l/2:d/(cmax+cmin),h=frac((4-dot(min(2,(cmax==c)+(c.gbr==c.rgb)+(c.gbr==c.brg)),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float lod=1+saturate(AlphaBlur)*log2(max(R.x,R.y));
    float4 c=tex2D(s0,x);
    c=float4(lerp(ShadowColor,c.rgb,c.a),max(c.a,Alpha*saturate(pow(2,Ext)*tex2Dlod(s0,float4(x+sqrt(2)*Offset*sin((Angle+float2(0,-0.25))*acos(-1)*2),0,lod)).a)));
    return c;
}
void vs2d(inout float4 vp:POSITION0){vp.xy*=2;}
technique Shadow{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
