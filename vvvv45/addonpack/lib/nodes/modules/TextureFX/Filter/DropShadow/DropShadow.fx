float2 R;
float Angle;
float Offset <float uimin=0.0; float uimax=1.0;> = 0.05;
float AlphaBlur <float uimin=0.0; float uimax=1.0;> = 0.1;
float Alpha <float uimin=0.0; float uimax=1.0;> = 0.7;
float Ext <float uimin=0.0;> = 0.0;
float4 ShadowColor:COLOR;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};

float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float lod=1+saturate(AlphaBlur)*log2(max(R.x,R.y));
    float4 c=tex2D(s0,x);
    c=float4(lerp(ShadowColor,c.rgb,c.a),max(c.a,Alpha*saturate(pow(2,Ext)*tex2Dlod(s0,float4(x+sqrt(2)*Offset*sin((Angle+float2(0,-0.25))*acos(-1)*2),0,lod)).a)));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Shadow{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
