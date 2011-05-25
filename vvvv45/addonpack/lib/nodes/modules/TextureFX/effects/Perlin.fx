float2 R;
float Octaves;
float Seed;
float Amplify;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    c=frac(x.x*18+Seed+x.y*8+length(sin(x*122+Seed+frac(x*198+Seed)))*4+2*sin(x.x*344.31+Seed+x.y*188+frac(x.x*5+Seed+x.y*398.15)*38.123*sin(x.y*98.12352+Seed)))*2-1;
    c.a=1;
    return c;
}
float4 p1(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;
    float sum=0;
    for (float i=0;i<min(ceil(Octaves),log2(max(R.x,R.y)));i++){
        float lod=log2(max(R.x,R.y))-i;
        float k=1;
        k=saturate(frac(Octaves)-(i-floor(Octaves)));
        c+=tex2Dlod(s0,float4(x,0,lod))*pow(2,lod)*.4*k;
        sum+=k;
    }
    c=.5+c/sum*Amplify;
    c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0){vp.xy*=2;}
technique ColorMap{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}pass pp1{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p1();}}
