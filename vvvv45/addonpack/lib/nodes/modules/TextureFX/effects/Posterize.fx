float2 R;
float4 levels;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    c=floor(c*max(levels,0))/(max(levels,0)+.000000001);
    c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0){vp.xy*=2;}
technique ColorMap{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
