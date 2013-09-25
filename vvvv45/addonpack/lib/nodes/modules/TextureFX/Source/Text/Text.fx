float4x4 tw:WORLDVIEWPROJECTION;
float2 R;
float4 TextColor:COLOR;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 psJoin(float2 x:TEXCOORD0):color{
    float4 c=TextColor;

    //c=TextColor;
    return c;
}
void vs2d(inout float4 vp:POSITION0){vp=mul(vp,tw);vp.xy*=1;}
technique TJoin{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 psJoin();}}
