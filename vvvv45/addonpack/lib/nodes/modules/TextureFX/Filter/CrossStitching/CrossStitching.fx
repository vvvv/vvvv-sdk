float2 R;
float stitching_size <float uimin=0.0; string uiname="Stiching Size";> = 20.0;
int invert <String uiname="Invert";> = 0;
float4 ColorA:COLOR <String uiname="Stich Color";>  = {0, 0, 0, 1};

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 Stitching(sampler2D tex, float2 uv)
{
  float4 c = float4(0.0, 0.0, 0.0, 0.0);
  float size = stitching_size;
  float2 cPos = uv * float2(R.x, R.y);
  float2 tlPos = floor(cPos / float2(size, size));
  tlPos *= size;
  int remX = int(cPos.x% size);
  int remY = int(cPos.y% size);
  if (remX == 0 && remY == 0)
  tlPos = cPos;
  float2 blPos = tlPos;
  blPos.y += (size - 1.0);
  if ((remX == remY) ||
     (((int(cPos.x) - int(blPos.x)) == (int(blPos.y) - int(cPos.y)))))
  {
    if (invert == 1)
      c = ColorA;
    else
      c = tex2D(tex, tlPos * float2(1.0/R.x, 1.0/R.y)) * 1.4;
  }
  else
  {
    if (invert == 1)
      c = tex2D(tex, tlPos * float2(1.0/R.x, 1.0/R.y)) * 1.4;
    else
      c = ColorA;
  }
  return c;
}

float4 p0(float2 vp : vpos): COLOR
{
	float2 uv=(vp+0.5)/R;
	return Stitching(Samp, uv);	
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique CrossStitching{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
