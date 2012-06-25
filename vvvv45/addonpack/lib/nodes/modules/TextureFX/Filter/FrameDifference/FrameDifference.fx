float2 R;
float4 Boost=0;
bool Signed=false;
bool Alpha=false;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=NONE;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=NONE;MinFilter=POINT;MagFilter=POINT;};
float4 lm(float4 c){c.rgb=dot(c.rgb,normalize(float3(.33,.59,.11))/1.5);return c;}
float4 p0(float2 x:TEXCOORD0):color{
    float4 pre=tex2D(s1,x);
    float4 cur=tex2D(s0,x);
	float4 c=(pre-cur)*pow(2,Boost);
	if(!Signed)c=abs(c);
	if(!Alpha)c.a=cur.a;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique FrameDifference{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
