float2 R;
float Sharp <float uimin=0.0;>;
float MinRadius <float uimin=0.0; float uimax=1.0;> = 00;
float Radius <float uimin=0.0; float uimax=1.0;> = 0.9;
float Saturation <float uimin=0.0; float uimax=1.0;> = 0.2;
float Gamma <float uimin=-1.0; float uimax=1.0;> = 0.2;
float Balance;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
    c=tex2Dlod(s0,float4(x,0,1));
    float maxl=log2(max(R.x,R.y))+.5;
    float4 sh=0;

    for (float i=0;i<7;i++){
    sh.rgb+=(tex2Dlod(s0,float4(x,0,1+maxl*saturate(Radius*lerp(i,7,MinRadius)/7)))-tex2Dlod(s0,float4(x,0,1+maxl*saturate(Radius*(lerp(i,7,MinRadius)+1)/7))))/pow(2,lerp(i,7,MinRadius)*Balance)*32*Sharp/pow(2,Radius/max(R.x,R.y)*pow(2,maxl*Radius));
    }
    sh.rgb=lerp(dot(sh.rgb,1.)/3.,sh.rgb,Saturation);
    //sh.rgb*=pow(2,Balance*7);
    //sh.rgb/=pow(2,lerp(Balance*0,-Balance*6,Balance<0));
    sh.rgb=sign(sh.rgb)*pow(abs(sh.rgb)*5,pow(2,Gamma*2))/5;
    sh/=1+c;
    //c.rgb*=pow(2,sh);
    c.rgb+=sh.rgb*pow(pow(2,3*min(Balance,0)),2);
    return c;
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
