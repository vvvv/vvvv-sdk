float2 R;
float Direction;
float Width;
float MapSmooth <float uimin=0.0; float uimax=1.0;> = 0.1;
float Twirl <float uimin=0.0; float uimax=1.0;> = 0.25;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float lod=1+saturate(MapSmooth)*log2(max(R.x,R.y));
    float4 c=0;
    float2 off=.25/R*pow(2,lod)*saturate(lod-1)+1./R;
    float4 map=tex2Dlod(s1,float4(x,0,lod));
    float ang=abs(map.x-.5)*Twirl*4;
    float2 dir=sin((ang+Direction+float2(0,.25))*acos(-1)*2);

    c=tex2D(s0,x+dir*R.x/R*.1*Width);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
