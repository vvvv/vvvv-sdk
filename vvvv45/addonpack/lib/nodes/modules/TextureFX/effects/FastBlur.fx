float2 R;
float Width;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float lod=1+saturate(Width)*log2(max(R.x,R.y));
    float4 c=0;
    float2 off=.25/R*pow(2,lod)*saturate(lod-1);
    c+=tex2Dlod(s0,float4(x+float2(0,0)*off,0,lod));
    c+=tex2Dlod(s0,float4(x+float2(1,1)*off,0,lod));
    c+=tex2Dlod(s0,float4(x+float2(1,-1)*off,0,lod));
    c+=tex2Dlod(s0,float4(x+float2(-1,-1)*off,0,lod));
    c+=tex2Dlod(s0,float4(x+float2(-1,1)*off,0,lod));
    off*=1.6;
    c+=tex2Dlod(s0,float4(x+float2(0,1)*off,0,lod));
    c+=tex2Dlod(s0,float4(x+float2(0,-1)*off,0,lod));
    c+=tex2Dlod(s0,float4(x+float2(-1,0)*off,0,lod));
    c+=tex2Dlod(s0,float4(x+float2(1,0)*off,0,lod));
    c/=9;
    return c;
}

void vs2d(inout float4 vp:POSITION0){vp.xy*=2;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
