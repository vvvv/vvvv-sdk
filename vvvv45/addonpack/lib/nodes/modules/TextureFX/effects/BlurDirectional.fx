float2 R;
float4 Levels;
bool Alpha;
float Dither;
float3 BlurVector;
float Rotate;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float2 r2d(float2 x,float a){return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
    float kk=0;
    float dith=(1+Dither*dot(vp%2,.5*pow(2,length(float4(BlurVector,Rotate*.5)))));
    for (float i=0;i<1;i+=1./16){
        c+=tex2D(s0,r2d(x-.5,i*Rotate*sqrt(dith))*(1-(BlurVector.z)*i)+.5+BlurVector.xy*(i-.5)*dith);
        kk++;
    }
    c=c/kk;
    return c;
}
void vs2d(inout float4 vp:POSITION0){vp.xy*=2;}
technique Posterize{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
