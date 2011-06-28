float2 R;
int2 PixelSize=(32,32);
bool Alpha=false;
bool Point=false;
float Smooth <float uimin=0.0; float uimax=1.0;> = 0.0;
float2 Scale <float uimin=0.0; float uimax=10.0;> =(1.0,1.0);
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float mx(float3 p){return max(p.x,max(p.y,p.z));}
float4 p0(float2 x:TEXCOORD0):color{
    float2 vp=x*R-.25;
    float2 sz=min(max(0.5/R,PixelSize),R);
    float4 c=tex2D(s0,floor(vp/sz)*sz/R+.5/R);
    float glow=length((frac(vp/sz)-.5)/Scale);
    float grey=mx(c.rgb);

    if(Point)grey=1;
    float circ=smoothstep(.48,.47*saturate(1-Smooth)*min(1,1-fwidth(glow)*1.6*saturate(PixelSize*.5)),glow/grey);
    c.rgb=c.rgb/grey*circ;
    if(Alpha)c.a*=circ;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Posterize{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
