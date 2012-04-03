float2 R;
bool Aspect;
float BlurX <float uimin=-1.0; float uimax=1.0;> = 0.1;
float BlurY <float uimin=-1.0; float uimax=1.0;> = 0.0;
float Width=1;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;float2 asp=lerp(1,R.x/R,Aspect);
    float4 c=0;
    float kk=0;
    float wd=Width*tex2D(s1,x);
    for (float i=0;i<1;i+=1./16){
	float k=1;
        c+=tex2D(s0,((x-.5)/asp+2*float2(BlurX,BlurY)*wd*(i))*asp+.5)*k;
        kk+=k;
    }
    c=c/kk;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
