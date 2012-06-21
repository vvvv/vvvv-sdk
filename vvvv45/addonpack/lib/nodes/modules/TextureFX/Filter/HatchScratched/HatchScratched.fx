float2 R;

float Thickness = 5.0;
float Threshold = 5.0;
float4 ColorA:COLOR <String uiname="Hatch Color";>  = {1, 1, 1, 1};
float4 ColorB:COLOR<String uiname="Background Color";>  = {0, 0, 0, 1};

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float rand ( float2 co ){
    return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
}
float4 p0(float2 vp : vpos): COLOR{	
	float3 f;
	float2 uv=(vp+0.5)/R;
	float3 col =  tex2D(Samp, uv).rgb;
	if (((vp.x * vp.y)%Thickness) > col.r * Threshold){
		f = ColorA.rgb;		
	}
	if (((vp.x * vp.y)%Thickness) > col.g * Threshold){
		f = ColorA.rgb;		
	}
	if (((vp.x * vp.y)%Thickness) > col.b * Threshold){
		f = ColorA.rgb;		
	}
	if ((pow(sin(rand(vp.x)),2)%Thickness) > col.r * Threshold){
		f = ColorB.rgb;		
	}
	if ((pow(sin(rand(vp.y)),2)%Thickness) > col.g * Threshold){
		f = ColorB.rgb;		
	}
	if ((pow(sin(rand(vp)),2)%Thickness) > col.b * Threshold){
		f = ColorB.rgb;		
	}
	return float4(f, 1.0); 
}
void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique HatchScratched {pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_3_0 p0();}}
