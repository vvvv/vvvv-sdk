float2 R;
float Bright <float uimin=0.0;> = 0.5;
float Radius <float uimin=0.0;> = 1.0;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 q(float2 x,float2 off,float v){return tex2Dlod(s0,float4(x+off/R,0,1+v));}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float rad=max(Radius,0);
    float3 e=float3(1,-1,0)*rad;
    float v=log2(rad);
    float4 cx=q(x,e.xy,v)+q(x,e.xz,v)+q(x,e.xx,v)-q(x,e.yy,v)-q(x,e.yz,v)-q(x,e.yx,v);
    float4 cy=q(x,e.yy,v)+q(x,e.zy,v)+q(x,e.xy,v)-q(x,e.yx,v)-q(x,e.zx,v)-q(x,e.xx,v);
    float4 c=sqrt(cx*cx+cy*cy)*Bright*pow(2,rad/max(R.x,R.y))/sqrt(saturate(rad)+.001);
    c.a=tex2D(s0,x).a;
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
