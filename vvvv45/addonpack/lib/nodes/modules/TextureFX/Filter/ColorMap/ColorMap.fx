float2 R;
float4 fromBlack:COLOR;
float4 fromWhite:COLOR;
float4 toBlack:COLOR;
float4 toWhite:COLOR;
float2 Ramp;
bool useRamp;
float Grayscale;
texture tex0,tex1;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
    float gray=lerp(dot(c.xyz,1)/3.,max(c.x,max(c.y,c.z)),.8);
    if(Grayscale)c.xyz=lerp(c.xyz,gray,saturate(Grayscale));
    c=saturate((c-fromBlack)/(fromWhite-fromBlack))*(toWhite-toBlack)+toBlack;
    if (useRamp){
       c=(c-.5)*(Ramp.x-1)/Ramp.x+.5;
       float4 ry=floor(Ramp.x/2);
       float4 rx=c;
       c.r=tex2D(s1,float2(rx.x,(ry.x+.5)/Ramp.y)).r;
       c.g=tex2D(s1,float2(rx.y,(ry.y+.5)/Ramp.y)).g;
       c.b=tex2D(s1,float2(rx.z,(ry.z+.5)/Ramp.y)).b;
       c.a=tex2D(s1,float2(rx.w,(ry.w+.5)/Ramp.y)).a;
    }
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique ColorRamp{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
