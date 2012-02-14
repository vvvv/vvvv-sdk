float2 R;
float4 ColorA:COLOR = {0.0, 0.0, 0.0, 1};
float4 ColorB:COLOR = {1.0, 1.0, 1.0, 1};
float2 Gamma={-4.0,-4.0};
float2 ClampBody <float uimin=0.0; float uimax=1.0;> = {0.0,0.0};
float2 XY={0.0,0.0};
float2 Scale={0.0,0.0};
float Rotate=0;
float2 r2d(float2 x,float a){return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);}
float4 p0(float2 x:TEXCOORD0):color{
       x=r2d((x-.5-XY)/Scale,Rotate*acos(-1)*2)+.5;
       float4 c=pow(smoothstep(.5,.4999*saturate(ClampBody.x),abs(x.x-.5)),pow(2,Gamma.x))*pow(smoothstep(.5,.4999*saturate(ClampBody.y),abs(x.y-.5)),pow(2,Gamma.y));
       c=lerp(ColorA,ColorB,c);
       return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Pillow{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
