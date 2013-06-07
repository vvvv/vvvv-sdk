float2 R;
bool Aspect;
float BlurWidth <float uimin=-1.0; float uimax=1.0;> = 0.2;
float BlurDir <float uimin=-1.0; float uimax=1.0;> = 0.0;
float MapSmooth <float uimin=0.0; float uimax=1.0;> = 0.1;
float Width=1;
float4 BorderCol:COLOR ={0.0,0.0,0.0,1.0};
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};

float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;float2 asp=lerp(1,R.x/R,Aspect);
    float lod=1+saturate(MapSmooth)*log2(max(R.x,R.y));
    float4 c=0;
    float kk=0;
    float wd=pow(Width,.1)*.25*BlurWidth;//*tex2D(s1,x);
    float ang=abs(tex2D(s1,x).x-.5);
    float2 dir=sin((ang+BlurDir+float2(0,.25))*acos(-1)*2);
    float2 off=pow(2,MapSmooth*6)*R/R.x;
    dir=float2(tex2Dlod(s1,float4(x-off*float2(1,0)/R,0,lod)).g-tex2Dlod(s1,float4(x+off*float2(1,0)/R,0,lod)).g,tex2Dlod(s1,float4(x-off*float2(0,1)/R,0,lod)).g-tex2Dlod(s1,float4(x+off*float2(0,1)/R,0,lod)).g);

    dir=normalize(r2d(dir,BlurDir/2+.25))*pow(length(dir.xy),1)*158*pow(2,MapSmooth*6);
    for (float i=0;i<1;i+=1./16){
        float k=1;
        c+=tex2D(s0,((x-.5)/asp+wd*dir*wd*(i))*asp+.5)*k;
        kk+=k;
    }
    c=c/kk;
    //c.rgb=abs(tex2D(s1,x).x-.5)*18;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
