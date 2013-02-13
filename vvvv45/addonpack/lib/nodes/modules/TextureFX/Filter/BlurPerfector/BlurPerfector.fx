float2 R;
bool Aspect;
float Radius;
float Threshold=.2;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float diff(float3 a,float3 b){
	float3 c=abs(a.xyz-b.yzx);
	return max(c.x,max(c.y,c.z));
}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;float2 asp=lerp(1,R.x/R,Aspect);
	float4 map=tex2D(s1,x);map=max(map.x,max(map.y,map.z))*map.a;
    float4 c=0;
    float kk=0;
    float4 cc=tex2Dlod(s0,float4(x,0,1));
    for (float i=0;i<1;i+=1./16){
    	float2 off=sqrt(i+.05)*sin((i*3.5+float2(0,.25))*acos(-1)*2);
        float4 nc=tex2Dlod(s0,float4(x+off/48*Radius*asp*map.x,0,1));
    	float k=pow(smoothstep(Threshold,0,distance(nc.rgb,cc.rgb)),3);
    	c.rgb+=nc*k;
        kk+=k;
    }
    c=c/kk;
	if(kk<2)c=cc;
	c.a=cc.a;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}} 