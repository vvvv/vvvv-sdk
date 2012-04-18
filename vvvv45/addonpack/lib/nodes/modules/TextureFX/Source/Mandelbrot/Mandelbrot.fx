float2 R;
float Iterations;
bool Mandel;
float2 Scale;
float2 XY;
float2 C;
float Zoom;
float Rotate;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float2 r2d(float2 x,float a){a*=acos(-1)*2;return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float4 p0(float2 vp:vpos,float2 xx:TEXCOORD0):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float2 xy=r2d((xx-.5)*R/R.y,Rotate);
    float2 u,z=(xy*Scale/pow(2,Zoom*16)-XY);
    float ss=length(z);
    ss=1;
    //z=normalize(z);
    u=z;

    bool stop=false;
    float g=0;
    for (float i=0;i<Iterations&&!stop;i++){
        z=((float2((z.x*ss)*(z.x*ss)-(z.y*ss)*(z.y*ss),2*(z.x*ss)*(z.y*ss))+ss*lerp(u,C,Mandel)))/ss;
        stop=length(z*ss>2);
        g=lerp(g,g+1,smoothstep(2,0,length(z*ss)));
    }
    //g=lerp(g,g+frac(Iterations),smoothstep(2,0,length(float2(z.x*z.x-z.y*z.y,2*z.x*z.y)+lerp(u,C,Mandel))));
    c=g/Iterations;

   // if(!stop)c=length(z);
    //c.r=xx.x;
    c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Fractal{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
