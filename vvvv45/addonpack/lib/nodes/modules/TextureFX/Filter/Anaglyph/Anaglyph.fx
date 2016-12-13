float2 R;
float Boost;
float Dist <float uimin=-1.0; float uimax=2.0;> = 0.1;
float MapBlur <float uimin=0.0; float uimax=1.0;> = 0.1;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 x:TEXCOORD0):color{
    float lod=1+saturate(MapBlur)*log2(max(R.x,R.y));
    float2 off=float2(Boost,0)/256.;
    float4 map=tex2Dlod(s1,float4(x,0,lod));
    float depth=map.x-Dist;
    float4 c=0;
    c.ra+=tex2D(s0,x-off*depth).ra;
    c.gba+=tex2D(s0,x+off*depth).gba;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Anaglyph{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
