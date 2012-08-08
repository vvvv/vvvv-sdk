float2 R;
float time;
float Rotate = 0.0;
float Dist <float uimin = 0.0;> = 0.5;
float Offset = 0.0;
float FogDistance <float uimin = 0.0;> = 0.5;

float4 ColorA:COLOR <String uiname="Fog Color";>  = {0, 0, 0, 1};

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float2 r2d(float2 x,float a)
{
	a*=acos(-1)*2;
	return float2(cos(a)*x.x+sin(a)*x.y,cos(a)*x.y-sin(a)*x.x);
}

float4 pTUNNELSQUARE(float2 vp : vpos): COLOR
{
	float2 p = -1.0 + 2.0 * vp.xy / R.xy;
	float2 pRot = r2d(p, Rotate);
    float2 uv;
    float r = pow(pow(pRot.x*pRot.x,3) + pow(pRot.y*pRot.y,3), 1.0/(Dist*8));
    uv.x = .5*time + 0.5/r;
	uv.y = (atan2(pRot.y,pRot.x)/3.1416+Offset);
    float4 col = tex2D(Samp,uv);
	
	return lerp(col, ColorA, 1/(1+(FogDistance*10.0)*pow(r/Dist,2)));
}

float4 pTUNNELCYLINDER(float2 vp : vpos): COLOR
{
	float2 p = -1.0 + 2.0 * vp.xy / R.xy;
	float2 uv;
	float a = atan2(p.y,p.x);
	float r = sqrt(dot(p,p))*(1-Dist);
	uv.x = .5*time+.5/r;
	uv.y = (a/(3.1416))+Rotate;
	float4 col =  tex2D(Samp,uv);
	return lerp(col, ColorA, 1/(1+(FogDistance*10.0)*pow(r/Dist,2)));
}

float4 pTUNNELFLY(float2 vp : vpos): COLOR
{
	float2 p = -1.0 + 2.0 * vp.xy / R.xy;
	float2 uv;
	float an = Rotate;
    float x = p.x*cos(an)-p.y*sin(an);
    float y = p.x*sin(an)+p.y*cos(an);  	
    uv.x = .25*x/abs(y)*Dist+(Offset+0.5);
    uv.y = .25*time + .25/abs(y)*Dist;	
	float4 col = tex2D(Samp,uv);
	return lerp(col,ColorA, 1/(1+(FogDistance*10.0)*pow(y/Dist,2)));
}

void vs2d(inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique TunnelSquare {pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pTUNNELSQUARE();}}
technique TunnelCylinder {pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pTUNNELCYLINDER();}}
technique TunnelFly {pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pTUNNELFLY();}}
