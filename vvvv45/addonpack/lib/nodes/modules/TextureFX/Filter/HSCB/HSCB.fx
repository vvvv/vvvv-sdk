float2 R;
float Hue;
float Saturation;
float Contrast;
float Brightness;
float HueCycles;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float3 hsv2rgb(float3 h){h.z+=max(0,h.y-1);float3 c=saturate((abs(frac(h.x+float3(1,2,3)/3)*6-3)-1))*h.z;c=lerp(c,max(c.r,max(c.g,c.b)),1-h.y);return c;}
float3 rgb2hsv(float3 c){float cmax=max(c.r,max(c.g,c.b)),cmin=min(c.r,min(c.g,c.b)),l=cmax,d=(cmax-cmin),s=max(abs(c.r-c.g),max(abs(c.g-c.b),abs(c.r-c.b)))/cmax,h=frac((4-dot(min(2,(cmax==c)),(c.gbr-c.brg)/d+float3(0,2,4)))/6);if(cmax==cmin)h=s=0;return float3(h,s,l);}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float3 h=rgb2hsv(c.rgb);
    h.x=h.x*HueCycles+Hue;
    h.y=h.y*Saturation;
    c.rgb=hsv2rgb(h);
    c.rgb=normalize(c.rgb)*sqrt(3)*pow(length(c.rgb)/sqrt(3),pow(2,Contrast))*pow(2,Brightness);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique HSCB{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
