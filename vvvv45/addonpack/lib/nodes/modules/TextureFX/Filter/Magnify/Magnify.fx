float2 R;

float2 center = 0.5;
float inner_radius;
float magnification;
float outer_radius;

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state  {Texture=(Tex);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 vp : vpos): COLOR
{
	float2 x=(vp+0.5)/R;
	float2 center_to_pixel = x - center;
	float distance = length(center_to_pixel);
	float4 color;
	float2 sample_point;
	
	if (distance < outer_radius)
	{
		if (distance < inner_radius)
		{
			sample_point = center + (center_to_pixel/magnification);
		}
		else 
		{
			float radius_diff = outer_radius - inner_radius;
			float ratio = (distance - inner_radius)/radius_diff; // 0==inner radius 1==outer_radius
			ratio = ratio * 3.14159;
			float adjusted_ratio = cos(ratio);
			adjusted_ratio = adjusted_ratio + 1;
			adjusted_ratio = adjusted_ratio / 2;
			sample_point = ((center+(center_to_pixel / magnification))*(adjusted_ratio))+(x*(1-adjusted_ratio));
		}
	}
	else
	{
		sample_point = x;
	}
	return tex2D(Samp, sample_point);	
}

void vs2d( inout float4 vp:POSITION, inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=0.5/R;}
technique Magnify{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_3_0 p0();}}
