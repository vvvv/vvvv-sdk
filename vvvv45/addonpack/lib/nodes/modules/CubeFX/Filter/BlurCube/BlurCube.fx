float2 R;
float Width <float uimin=0.0; float uimax=1.0;> =.5;
//float Parameter <float uimin=0.0; float uimax=1.0;> =0;
float4x4 tWVP: WORLDVIEWPROJECTION;  
float4x4 tr;
texture tex0;
samplerCUBE s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;AddressW=CLAMP;};
//float linstep(float a,float b,float x){return saturate((x-a)/(b-a));}
#define linstep smoothstep
float mx2(float2 x){x=abs(x);return max(x.x,x.y);}
float3 mx(float3 x){return max(x.x,max(x.y,x.z));}
float4 LevelBlend(samplerCUBE s,float3 x,float lod){
	float MaxLOD=log2(max(R.x,R.y));
	lod+=1;
	float4 c=0;
	float3 cx=x/max(length(x.x),max(length(x.y),length(x.z)));
	c+=texCUBElod(s,float4(x.xyz,lod));
	float ed=(lod-1)/MaxLOD;
	ed=1-ed*ed*ed;
	ed=max(ed,.25);
	float3 p=normalize(x)/mx(abs(x));
	float px=ed*2.001;
	if(mx2(p.xy)>px&&mx2(p.xz)>px&&mx2(p.zy)>px){
	c+=texCUBElod(s,float4(x.zyx*float3( 1, 1, 1),lod))*linstep( ed, 1,cx.z)*linstep( ed, 1,cx.x);
	c+=texCUBElod(s,float4(x.zyx*float3( 1, 1, 1),lod))*linstep(-ed,-1,cx.z)*linstep(-ed,-1,cx.x);
	c+=texCUBElod(s,float4(x.zyx*float3(-1, 1,-1),lod))*linstep( ed, 1,cx.z)*linstep(-ed,-1,cx.x);
	c+=texCUBElod(s,float4(x.zyx*float3(-1, 1,-1),lod))*linstep(-ed,-1,cx.z)*linstep( ed, 1,cx.x);
	c+=texCUBElod(s,float4(x.xzy*float3( 1, 1, 1),lod))*linstep( ed, 1,cx.z)*linstep( ed, 1,cx.y);
	c+=texCUBElod(s,float4(x.xzy*float3( 1, 1, 1),lod))*linstep(-ed,-1,cx.z)*linstep(-ed,-1,cx.y);
	c+=texCUBElod(s,float4(x.xzy*float3( 1,-1,-1),lod))*linstep( ed, 1,cx.z)*linstep(-ed,-1,cx.y);
	c+=texCUBElod(s,float4(x.xzy*float3( 1,-1,-1),lod))*linstep(-ed,-1,cx.z)*linstep( ed, 1,cx.y);
	c+=texCUBElod(s,float4(x.yxz*float3( 1, 1, 1),lod))*linstep( ed, 1,cx.x)*linstep( ed, 1,cx.y);
	c+=texCUBElod(s,float4(x.yxz*float3( 1, 1, 1),lod))*linstep(-ed,-1,cx.x)*linstep(-ed,-1,cx.y);
	c+=texCUBElod(s,float4(x.yxz*float3(-1,-1, 1),lod))*linstep( ed, 1,cx.x)*linstep(-ed,-1,cx.y);
	c+=texCUBElod(s,float4(x.yxz*float3(-1,-1, 1),lod))*linstep(-ed,-1,cx.x)*linstep( ed, 1,cx.y);
		//c.r=1;
	}
	c.xyz/=c.a;
	c.a=texCUBElod(s,float4(x.xyz,1)).a;
	
	return c;
}
float ld=8;
float4 p0(float2 x:TEXCOORD0,float3 p:TEXCOORD1):color{
    float4 c=1;
	float2 uv=float2(atan2(p.z,p.x)/acos(-1)/2+.75,acos(p.y*2)/acos(-1));
	//c.rgb=sin((c.rgb/4+Parameter)*acos(-1)*2);
	c=0;

	//c/=c.a;
	c=LevelBlend(s0,mul(float4(p,1),tr).xyz,ld);
	c=0;
	float pw=mx(float3(mx2(p.xy),mx2(p.zy),mx2(p.xz)))*2-.5;
	for(float i=0;i<25;i++){
		float3 off=sqrt(i/25.)*normalize(sin((i+1)*sqrt(float3(5,6,7))));
		c+=(texCUBElod(s0,float4(normalize(p.xyz+off*.5*Width),1+Width*28)));
		
	}
	
	float3 cp=normalize(p)/mx(abs(p));
	c/=c.a;
	//c.rgb=texCUBElod(s0,float4(p.xyz,1+ld*(.3+.7*pw)));
	//c.rgb=pw;
	c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,inout float3 p:TEXCOORD1){p=vp;vp=mul(vp,tWVP);}
technique TBlur{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
