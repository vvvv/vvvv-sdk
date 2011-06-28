float2 R;
float4 ColorA:COLOR = {0.0, 0.0, 0.0, 1};
float4 ColorB:COLOR = {1.0, 1.0, 1.0, 1};
float Radius;
float4x4 tWVP: WORLDVIEWPROJECTION;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=POINT;MinFilter=POINT;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
struct vs2ps
{
   float4 Pos: POSITION;
   float4 Data: TEXCOORD0;
   float4 Col: COLOR0;
   float Size:PSIZE;
};
vs2ps v0(float4 pos:POSITION){

    vs2ps Out=(vs2ps)0;

    Out.Pos = mul(pos*float4(min(R.x,R.y)/R,1,1), tWVP);
    Out.Size=saturate(Radius*max(R.x,R.y)*.3/500.)*500.;
    return Out;
}
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=1-saturate(1-2*length(x-.5));
    //c=length(x-.5)*2*ColorA;
    c=lerp(ColorB,ColorA,saturate(1-2*length(x-.5)));
    //c.a=1;
    //c=lerp(ColorA,ColorB,c);
    return c;
}
technique Voronoi{pass pp0{FillMode=POINT;PointSpriteEnable=TRUE;ZWriteEnable=FALSE;AlphaBlendEnable=TRUE;SrcBlend=ONE;DestBlend=ONE;BlendOp=MIN;vertexshader=compile vs_2_0 v0();pixelshader=compile ps_2_0 p0();}}
