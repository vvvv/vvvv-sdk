float2 R;
float time;
float4 ColorA : COLOR <String uiname="Electricity Color"; >  = {1, 1, 1, 1};
float4 ColorB : COLOR <String uiname="Background Color"; >  = {0, 0, 0, 1};
float glowStrength = 40;
float ambientGlow = 0.5;
float ambientGlowHeightScale = 1.0;
float height = 0.25;
float glowFallOff = 0.02;
float speed = 0.15;
float vertexNoise = 0.5;

texture Noise_Tex;
sampler Noise = sampler_state{Texture = (Noise_Tex);ADDRESSU = WRAP;ADDRESSV = WRAP;ADDRESSW = WRAP;MAGFILTER = LINEAR;MINFILTER = LINEAR;MIPFILTER = LINEAR;};

float4 p0(float2 vp: TEXCOORD) : COLOR
{
	float2 uv = vp-0.5;
	float2 t = float2(speed * time * 0.5871 - vertexNoise * abs(uv.y), speed * time);

	// Sample at three positions for some horizontal blur
	// The shader should blur fine by itself in vertical direction
	float xs0 = uv.x;
	float xs1 = uv.x;
	float xs2 = uv.y + uv.x ;

	// Noise for the three samples
	float noise0 = tex3D(Noise, float3(xs0, t));
	float noise1 = tex3D(Noise, float3(xs1, t));
	float noise2 = tex3D(Noise, float3(xs2, t));

	// The position of the flash
	float mid0 = height * (noise0 * 2 - 1) * (1 - xs0 * xs0);
	float mid1 = height * (noise1 * 2 - 1) * (1 - xs1 * xs1);
	float mid2 = height * (noise2 * 2 - 1) * (1 - xs2 * xs2);

	// Distance to flash
	float dist0 = abs(uv.y - mid0);
	float dist1 = abs(uv.y - mid1);
	float dist2 = abs(uv.y - mid2);

	// Glow according to distance to flash
	float glow = 1.0 - pow(0.25 * (dist0 + 2 * dist1 + dist2), glowFallOff);

	// Add some ambient glow to get some power in the air feeling
	float ambGlow = ambientGlow * (1 - xs1 * xs1) * (1 - abs(ambientGlowHeightScale * uv.y));

	return lerp(ColorB, ColorA, (glowStrength * glow * glow + ambGlow));
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Electricity{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
