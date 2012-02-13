float2 R;
float3 hsv2rgb(float3 h){h.z+=max(0,h.y-1);float3 c=saturate((abs(frac(-h.x+float3(3,1,2)/3)*6-3)-1))*h.z;c=lerp(c,max(c.r,max(c.g,c.b)),1-h.y);return c;}
float3 rgb2hsv(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=cmax,d=(cmax-cmin),s=max(abs(c.r-c.g),max(abs(c.g-c.b),abs(c.r-c.b)))/cmax,h;float3 ch=(cmax==c);if(ch.r==ch.g&&ch.r==1)ch.rg=float2(1,0);if(ch.g==ch.b&&ch.g==1)ch.gb=float2(1,0);if(ch.b==ch.r&&ch.b==1)ch.br=float2(1,0);h=frac((dot(min(2,ch),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}
float3 hsl2rgb(float3 h){return lerp(h.z,saturate((abs(frac(-h.x+float3(3,1,2)/3)*6-3)-1))*h.z*2,saturate(h.y)*saturate(2-2*h.z));}
float3 rgb2hsl(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=(cmax+cmin)/2,d=(cmax-cmin),s=l>.5?d/(2-cmax-cmin)/l/2:d/(cmax+cmin),h;float3 ch=(cmax==c);if(ch.r==ch.g&&ch.r==1)ch.rg=float2(1,0);if(ch.g==ch.b&&ch.g==1)ch.gb=float2(1,0);if(ch.b==ch.r&&ch.b==1)ch.br=float2(1,0);h=frac((dot(min(2,ch),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}
float2 r2d(float2 x,float a){return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float4 pHSL(float2 x:TEXCOORD0):color{
       float4 c=1;
       c.rgb=hsv2rgb(float3(x.x,1,saturate(2-2*x.y)));
       c.rgb=lerp(c.rgb,max(c.r,max(c.g,c.b)),saturate(1-x.y*2));
       return c;
}
float4 pHSV(float2 x:TEXCOORD0):color{
       float4 c=1;
       c.rgb=hsv2rgb(float3(x.x,1,1-x.y));
       return c;
}
float4 pRADIAL(float2 x:TEXCOORD0):color{
       float4 c=1;
       c.rgb=float3(x.x,x.y,length(x));
       c.rgb=hsl2rgb(float3(atan2(x.y-.5,x.x-.5)/acos(-1)/2,1,length(x-.5)));
       return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique XY_HSV{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pHSV();}}
technique XY_HSL{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pHSL();}}
technique RADIAL_HSV{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 pRADIAL();}}
