float2 R;
float matr[25];
float Multiplier=1;
bool Alpha=false;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p3x3(float2 x:TEXCOORD0):color{
    float4 c=tex2D(s0,x);float pa=c.a;
    c*=matr[4];
    float2 off=1./R;
    c+=matr[0]*tex2D(s0,x-float2( 1, 1)*off);
    c+=matr[1]*tex2D(s0,x-float2( 0, 1)*off);
    c+=matr[2]*tex2D(s0,x-float2(-1, 1)*off);
    c+=matr[3]*tex2D(s0,x-float2( 1, 0)*off);
    c+=matr[5]*tex2D(s0,x-float2(-1, 0)*off);
    c+=matr[6]*tex2D(s0,x-float2( 1,-1)*off);
    c+=matr[7]*tex2D(s0,x-float2( 0,-1)*off);
    c+=matr[8]*tex2D(s0,x-float2(-1,-1)*off);
    c*=Multiplier;
    if(!Alpha)c.a=1;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Convolution3x3{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p3x3();}}
