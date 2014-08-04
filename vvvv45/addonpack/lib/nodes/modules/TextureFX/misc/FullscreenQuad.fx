float4x4 tW: WORLD;
float4x4 tV: VIEW;
float4x4 tP: PROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;
//material properties
float4 Color:COLOR = {1,1,1,1};
//texture
texture tex0 <string uiname="Texture";>;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=WRAP;AddressV=WRAP;};


float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;
float4x4 tColor <string uiname="Color Transform";>;

float Z <float uimin=0.0; float uimax=1.0;> =0;

float4 PS(float2 uv:TEXCOORD0): COLOR
{
    float4 c=tex2D(s0,uv);
	c = mul(c,tColor);
	c=c*Color;
    return c;
}

void VS(inout float4 vp:POSITION0,inout float4 uv:TEXCOORD0){vp.xy*=2;vp=mul(vp,tWVP);vp.z=Z;uv=mul(uv,tTex);}


technique Off{
	pass P0{
		AlphaBlendEnable=FALSE;
		VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();
	}
}
technique Blend{
	pass P0{
		AlphaBlendEnable=TRUE;
		BlendOp=ADD;
		SrcBlend=SRCALPHA;
		DestBlend=INVSRCALPHA;
		VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();
	}
}
technique Add{
	pass P0{
		AlphaBlendEnable=TRUE;
		BlendOp=ADD;
		SrcBlend=ONE;
		DestBlend=ONE;
		VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();
	}
}
technique Screen{
	pass P0{
		AlphaBlendEnable=TRUE;
		BlendOp=ADD;
		SrcBlend=INVDESTCOLOR;
		DestBlend=ONE;
		VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();
	}
}
technique Multiply{
	pass P0{
		AlphaBlendEnable=TRUE;
		BlendOp=ADD;
		SrcBlend=DESTCOLOR;
		DestBlend=ZERO;
		VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();
	}
}
technique MultiplyX2{
	pass P0{
		AlphaBlendEnable=TRUE;
		BlendOp=ADD;
		SrcBlend=DESTCOLOR;
		DestBlend=SRCCOLOR;
		VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();
	}
}
technique Max{
	pass P0{
		AlphaBlendEnable=TRUE;
		BlendOp=MAX;
		SrcBlend=ONE;
		DestBlend=ONE;
		VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();
	}
}
technique Min{
	pass P0{
		AlphaBlendEnable=TRUE;
		BlendOp=MIN;
		SrcBlend=ONE;
		DestBlend=ONE;
		VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();
	}
}
//technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
//technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
//technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
//technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}