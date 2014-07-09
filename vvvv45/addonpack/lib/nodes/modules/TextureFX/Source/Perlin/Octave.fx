float2 R;
float OctaveCount;
float Octave;
float3 Offset;
float Seed;
float Amplify;
float Balance;
bool Repeat;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

//float3 nois(float3 p){p+=sin(p.yzx*sqrt(float3(3,5,6))*8);return frac(sin(sqrt(99*float3(5,6,7)*p)*9*sin(p.yzx*sqrt(88*float3(17,14,13)+2))));}
float nois(float3 p){p+=sin(p.yzx*19)/6;p=frac(p*3*sqrt(float3(15,16,17)));p+=tex2D(s0,p.xy+p.z*length(p.xy));p+=sin(p.yzx*sqrt(float3(5,6,7)*88));p+=tex2D(s0,p.xy+p.z*length(p.xy));p=frac(p*sqrt(float3(5,6,7)));p+=sin(p.yzx*8);p=frac(p*18);return p.x;}
//float nois(float3 p){p*=889;p+=tex2D(s0,p.xy+.5+frac(27*tex2D(s0,p.yz+.3+frac(8*tex2D(s0,p.zx+.6)))))*28;p=frac(p);return p.x;}
float4 p0(float2 vp:vpos):color{
    clip(OctaveCount-Octave);
    if(Repeat&&vp.x==R.x-1)vp.x=0;
    if(Repeat&&vp.y==R.y-1)vp.y=0;
    float2 x=(vp+.0)/R;
    float c=0;
    float Zs=(max(R.x,R.y));
    //float Z=frac(Seed)*Zs;
    float Z=Offset.z*Zs;
    //Z=frac(Z/Zs)*Zs;
    float Zi=floor(Z);
    float Zf=frac(Z);
    c=lerp(nois(float3(x,Zi)),nois(float3(x,Zi+1)),Zf).r*2-1;
    c*=pow(2,Octave*Balance)/pow(2,OctaveCount*Balance)*.1;
    
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Perlin{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
