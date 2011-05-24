float2 R;
float Sharp;
float Radius;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
    c=tex2Dlod(s0,float4(x,0,1));
    float maxl=log2(max(R.x,R.y))*saturate(Radius);
    for (float i=0;i<7;i++){
        c+=(tex2Dlod(s0,float4(x,0,1+i*maxl))-tex2Dlod(s0,float4(x,0,1+(i+1)*maxl)))*Sharp*8/sqrt(Radius)*pow(2,i*-.7);
    }

    return c;
}

void vs2d(inout float4 vp:POSITION0){vp.xy*=2;}
technique BlackBorder{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique RepeatEdgePixels{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
