float2 R;
float time;
float SizeX = 2.0;
float SizeY = 2.0;
float Deepness = 5.0;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{
	float2 p = -1.0 + 2.0 * vp.xy / R.xy;
    float2 uv;
    float r = pow( pow(p.x*p.x,(SizeX*4)) + pow(p.y*p.y,(SizeY*4)), 1.0/(Deepness*4));
    uv.x = .5*time + 0.5/r;
    uv.y = 1.0*atan2(p.y,p.x)/3.1416;
    float3 col =  tex2D(Samp,uv).xyz;
    return float4(col*r*r*r,1.0);
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}

technique TunnelSquare {pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_3_0 p0();}}
