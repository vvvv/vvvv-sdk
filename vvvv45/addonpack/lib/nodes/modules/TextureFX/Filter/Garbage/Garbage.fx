float2 R;
bool KeepSharp;
float2 Offset;
float Alpha;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
texture tex1;
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
texture tex2;
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
texture tex3;
sampler s3=sampler_state{Texture=(tex3);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float mx(float3 p){return max(p.x,max(p.y,p.z));}
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);float pa=c.a;
    float diff=abs(c-tex2D(s1,x));
    diff=mx((abs(c-tex2D(s1,x))*pow(2,Alpha)).xyz);
    float2 off=Offset;
    if(KeepSharp)off=round(off);
	float4 pre=tex2D(s2,x+off/R);
	//pre.rgb=1-saturate((pre.rgb-.5)*1.1+.5);
    c=lerp(pre,c,saturate(diff));
    //float4 e=tex2D(s3,x);
    //c=lerp(c,e,e.a);
    //c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Garbage{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
