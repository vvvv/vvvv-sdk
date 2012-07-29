float2 R;
float fromBlack <float uimin=0.0; float uimax=1.0;> =0;
float fromWhite <float uimin=0.0; float uimax=1.0;> =1;
float toBlack <float uimin=0.0; float uimax=1.0;> =0;
float toWhite <float uimin=0.0; float uimax=1.0;> =1;
float Gamma=1;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);
	c.a=saturate((c.a-fromBlack)/(fromWhite-fromBlack));
    c.a=(sign(c.a)*pow(abs(c.a),Gamma)*(toWhite-toBlack)+toBlack);
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique AlphaLevels{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
