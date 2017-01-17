float2 R;
bool Aspect;
float BlurX <float uimin=-1.0; float uimax=1.0;> = 0.2;
float BlurY <float uimin=-1.0; float uimax=1.0;> = 0.0;
float BlurZ <float uimin=0.0; float uimax=1.0;> = 0.0;
float BlurR <float uimin=-1.0; float uimax=1.0;> = 0.0;
float Width=1;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;float2 asp=lerp(1,R.x/R,Aspect);
    float4 c=0;
    float kk=0;
    float2 piv=.5+float2(BlurX,BlurY);
    float wd=Width*tex2D(s1,x).x;
    for (float i=0;i<1;i+=1./16){
        c+=tex2D(s0,r2d((x-piv)/asp,i*BlurR*wd)/pow(2,(BlurZ*wd)*6*i)*asp+piv);
        kk++;
    }
    c=c/kk;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
