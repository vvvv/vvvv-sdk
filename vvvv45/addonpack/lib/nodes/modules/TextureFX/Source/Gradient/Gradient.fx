float2 R;
float2 FromXY;
float2 ToXY;
float Extrapolate;
float4 ColorA:COLOR;
float4 ColorB:COLOR;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 psDir(float2 x:TEXCOORD0):color{
    float4 c0=tex2D(s0,x);
    float2 gx=dot(x-.5-FromXY*float2(1,-1)/2,float2(-1,1)*(FromXY-ToXY))*2/pow(length(FromXY-ToXY),2);
    float grad=gx.x;
    if(!Extrapolate)grad=saturate(grad);
    float4 c=saturate(lerp(ColorA,ColorB,grad));
    return c;
}
float4 psGlow(float2 x:TEXCOORD0):color{
    float4 c0=tex2D(s0,x);
    float grad=length(x-.5-FromXY*float2(1,-1)/2)*2/length(FromXY-ToXY);
    if(!Extrapolate)grad=saturate(grad);
    float4 c=saturate(lerp(ColorA,ColorB,grad));
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Linear{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 psDir();}}
technique Radial{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 psGlow();}}
