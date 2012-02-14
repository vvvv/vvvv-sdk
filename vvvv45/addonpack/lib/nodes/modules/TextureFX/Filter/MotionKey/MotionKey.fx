float2 R;
bool Alpha;
float Bright <float uimin=0.0;> = 1.0;
int FrameCount <float uimin=2.0;float uimax=16.0;> = 2.0;
texture tex0,tex1,tex2,tex3,tex4,tex5,tex6,tex7,tex8,tex9,tex10,tex11,tex12,tex13,tex14,tex15;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(tex2);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s3=sampler_state{Texture=(tex3);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s4=sampler_state{Texture=(tex4);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s5=sampler_state{Texture=(tex5);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s6=sampler_state{Texture=(tex6);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s7=sampler_state{Texture=(tex7);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s8=sampler_state{Texture=(tex8);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s9=sampler_state{Texture=(tex9);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s10=sampler_state{Texture=(tex10);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s11=sampler_state{Texture=(tex11);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s12=sampler_state{Texture=(tex12);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s13=sampler_state{Texture=(tex13);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s14=sampler_state{Texture=(tex14);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s15=sampler_state{Texture=(tex15);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);float pa=c.a;
    float4 m[16]={tex2D(s0,x),tex2D(s1,x),tex2D(s2,x),tex2D(s3,x),tex2D(s4,x),tex2D(s5,x),tex2D(s6,x),tex2D(s7,x),tex2D(s8,x),tex2D(s9,x),tex2D(s10,x),tex2D(s11,x),tex2D(s12,x),tex2D(s13,x),tex2D(s14,x),tex2D(s15,x)};
    float4 diff=0;
    for (float i=1;i<FrameCount;i++){
        diff+=abs(m[i-1]-m[i])*pow((FrameCount-i)/(float)FrameCount,0);
    }
    //c=diff;
    c.a=pow(2,Bright)*max(max(diff.x,diff.y),max(diff.z,diff.w))/FrameCount;
    //if(Alpha)c=float4(c.rgb*c.a,pa);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique FrameDiff{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
