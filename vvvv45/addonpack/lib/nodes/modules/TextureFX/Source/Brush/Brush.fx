float2 R;
float X;
float Y;
float BrushSize =.25;
float Hardness <float uimin=0.0; float uimax=1.0;> =0;
float4 BrushColor:COLOR =1;
float4 BackgroundColor:COLOR =(0,0,0,0);
bool ShowBrush =1;
//texture tex0;
//sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=0;//tex2D(s0,x);
	float d=length((x-.5-float2(X,-Y)/2)/R.x*R)/BrushSize;
	c=smoothstep(.5,.49999*Hardness-fwidth(d)*1,d);
    //c=float4(BrushColor.rgb,BrushColor.a*c.x*Draw);
	c=lerp(BackgroundColor,BrushColor,c.x*ShowBrush);
	return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Brush{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
