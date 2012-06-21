float2 R;
float time;
float Rotate = 0.0;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{
	float2 p = -1.0 + 2.0 * vp.xy / R.xy;
	float2 uv;
	float an = Rotate;
    float x = p.x*cos(an)-p.y*sin(an);
    float y = p.x*sin(an)+p.y*cos(an);     
    uv.x = .25*x/abs(y);
    uv.y = .25*time + .25/abs(y);
    return float4(tex2D(Samp,uv).xyz * y*y, 1.0);
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}

technique TunnelFly {pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_3_0 p0();}}
