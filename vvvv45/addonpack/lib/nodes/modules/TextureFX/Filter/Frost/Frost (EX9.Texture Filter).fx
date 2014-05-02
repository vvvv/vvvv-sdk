float2 R;
float PixelX <string uiname="AmountX";> = 5.0;
float PixelY <string uiname="AmountY";> = 5.0;
float Freq <float uimin=0.0; float uimax=1.0; string uiname="Frequency";> = 0.115;

texture tex0,tex1;

sampler sceneTex=sampler_state{Texture=(tex0);MipFilter=NONE;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler NoiseTex=sampler_state{Texture=(tex1);MipFilter=NONE;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 spline(float x, float4 c1, float4 c2, float4 c3, float4 c4, \
                float4 c5, float4 c6, float4 c7, float4 c8, float4 c9)
{
  float w1, w2, w3, w4, w5, w6, w7, w8, w9;
  w1 = 0.0;
  w2 = 0.0;
  w3 = 0.0;
  w4 = 0.0;
  w5 = 0.0;
  w6 = 0.0;
  w7 = 0.0;
  w8 = 0.0;
  w9 = 0.0;
  float tmp = x * 8.0;
  if (tmp<=1.0) {
    w1 = 1.0 - tmp;
    w2 = tmp;
  }
  else if (tmp<=2.0) {
    tmp = tmp - 1.0;
    w2 = 1.0 - tmp;
    w3 = tmp;
  }
  else if (tmp<=3.0) {
    tmp = tmp - 2.0;
    w3 = 1.0-tmp;
    w4 = tmp;
  }
  else if (tmp<=4.0) {
    tmp = tmp - 3.0;
    w4 = 1.0-tmp;
    w5 = tmp;
  }
  else if (tmp<=5.0) {
    tmp = tmp - 4.0;
    w5 = 1.0-tmp;
    w6 = tmp;
  }
  else if (tmp<=6.0) {
    tmp = tmp - 5.0;
    w6 = 1.0-tmp;
    w7 = tmp;
  }
  else if (tmp<=7.0) {
    tmp = tmp - 6.0;
    w7 = 1.0 - tmp;
    w8 = tmp;
  }
  else
  {
    tmp = clamp(tmp - 7.0, 0.0, 1.0);
    w8 = 1.0-tmp;
    w9 = tmp;
  }
  return w1*c1 + w2*c2 + w3*c3 + w4*c4 + w5*c5 + w6*c6 + w7*c7 + \
           w8*c8 + w9*c9;
}

float3 NOISE2D(float2 p){ 
	
	return tex2D(NoiseTex,p).xyz;
}

float4 p0 (float2 vp:vpos):COLOR {
	
	float2 uv=(vp+0.5)/R;
	float3 tc = float3(1.0, 0.0, 0.0);

	float DeltaX = PixelX / R.x;
	float DeltaY = PixelY / R.y;
	float2 ox = float2(DeltaX,0.0);
	float2 oy = float2(0.0,DeltaY);
	float2 PP = uv - oy;
	float4 C00 = tex2D(sceneTex,PP - ox);
	float4 C01 = tex2D(sceneTex,PP);
	float4 C02 = tex2D(sceneTex,PP + ox);
	PP = uv;
	float4 C10 = tex2D(sceneTex,PP - ox);
	float4 C11 = tex2D(sceneTex,PP);
	float4 C12 = tex2D(sceneTex,PP + ox);
	PP = uv + oy;
	float4 C20 = tex2D(sceneTex,PP - ox);
	float4 C21 = tex2D(sceneTex,PP);
	float4 C22 = tex2D(sceneTex,PP + ox);

	float n = NOISE2D(Freq*uv).x;
	n = (n % 0.111111)/0.111111;
	float4 result = spline(n,C00,C01,C02,C10,C11,C12,C20,C21,C22);
	tc = result.rgb;

	return float4(tc, 1.0);
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Frost{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}


