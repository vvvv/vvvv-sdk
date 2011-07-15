float2 R;
float X;
float Y;
float Zoom;
float Radius;
float Count;
bool Aspect=true;
texture tex0,tData;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler sD=sampler_state{Texture=(tData);MipFilter=POINT;MinFilter=POINT;MagFilter=POINT;};
float2 zom(float2 x,float a){x*=pow(2,a*8);return x;}
float mx(float2 p){return max(p.x,p.y);}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;float2 asp=lerp(1,R.x/R,Aspect);
    float2 dx=(x);
    float maxlod=log2(max(R.x,R.y));
    for (float i=0;i<min(Count,64);i++){
        float4 data=tex2Dlod(sD,float4((i+.5)/64,0.5,0,1));
        data.xy=data.xy*.5+.5;
        data.w=max(.0001,data.w);
        dx=(dx-data.xy)/asp;dx=zom(dx,data.z*pow(data.w-length(dx),.5)*smoothstep(1,0,length(dx)/data.w))*asp+data.xy;
    }
    float4 c=tex2Dlod(s0,float4(dx,0,min(maxlod,pow(length(fwidth(dx)/fwidth(x)),.5))));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique LensDistort{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
