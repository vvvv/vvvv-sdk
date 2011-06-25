float2 R;
float4 Levels;
bool Alpha;
float BlurX <float uimin=0.0; float uimax=1.0;> = 0.2;
float BlurY <float uimin=0.0; float uimax=1.0;> = 0.0;
float BlurZ <float uimin=0.0; float uimax=1.0;> = 0.0;
float BlurR <float uimin=0.0; float uimax=1.0;> = 0.0;
float Width=1;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
    float kk=0;
    for (float i=0;i<1;i+=1./16){
        c+=tex2D(s0,r2d(x-.5,i*BlurR*Width)/pow(2,(BlurZ*Width)*6*i)+.5+2*float2(BlurX,BlurY)*Width*(i-.5));
        kk++;
    }
    c=c/kk;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Posterize{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
