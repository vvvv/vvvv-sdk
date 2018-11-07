/*good piece of code by Ian Taylor (from http://www.chilliant.com/rgb2hsv.html)*/

float3 HUEtoRGB(in float H){
	H=frac(H);
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);
	return saturate(float3(R,G,B));
}
float3 HSVtoRGB(in float3 HSV){
	float3 RGB = HUEtoRGB(HSV.x);
	return ((RGB - 1) * HSV.y + 1) * HSV.z;
}
float3 HSLtoRGB(in float3 HSL)
{
	float3 RGB = HUEtoRGB(HSL.x);
	float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
	return (RGB - 0.5) * C + HSL.z;
}
float3 RGBtoHSV(in float3 RGB){
	float3 HSV = 0;
	HSV.z = max(RGB.r, max(RGB.g, RGB.b));
	float M = min(RGB.r, min(RGB.g, RGB.b));
	float C = HSV.z - M;
	if (C != 0){
		float4 RGB0 = float4(RGB, 0);
		float4 Delta = (HSV.z - RGB0) / C;
		Delta.rgb -= Delta.brg;
		Delta.rgb += float3(2,4,6);
		Delta.brg = step(HSV.z, RGB) * Delta.brg;
		HSV.x = max(Delta.r, max(Delta.g, Delta.b));
		HSV.x = frac(HSV.x / 6);
		HSV.y = 1 / Delta.w;
	}
	return HSV;
}
float3 RGBtoHSL(in float3 RGB){
	float3 HSL = 0;
	float U, V;
	U = -min(RGB.r, min(RGB.g, RGB.b));
	V = max(RGB.r, max(RGB.g, RGB.b));
	HSL.z = (V - U) * 0.5;
	float C = V + U;
	if (C != 0){
		float3 Delta = (V - RGB) / C;
		Delta.rgb -= Delta.brg;
		Delta.rgb += float3(2,4,6);
		Delta.brg = step(V, RGB) * Delta.brg;
		HSL.x = max(Delta.r, max(Delta.g, Delta.b));
		HSL.x = frac(HSL.x / 6);
		HSL.y = C / (1 - abs(2 * HSL.z - 1));
	}
	return HSL;
}