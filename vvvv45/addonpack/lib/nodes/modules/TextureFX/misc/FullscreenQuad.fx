float4x4 tW: WORLD;
float4x4 tV: VIEW;
float4x4 tP: PROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;
//material properties
float4 Color:COLOR = {1,1,1,1};
//texture
texture tex0 <string uiname="Texture";>;
sampler s0=sampler_state{Texture=(tex0);};
float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;
float4x4 tColor <string uiname="Color Transform";>;

float Z <float uimin=0.0; float uimax=1.0;> =0;

float4 PS(float2 uv:TEXCOORD0): COLOR
{
    float4 c=tex2D(s0,uv+.01/8192.);
	c = mul(c,tColor);
	c=c*Color;
    return c;
}

void VS(inout float4 vp:POSITION0,inout float4 uv:TEXCOORD0){vp.xy*=2;vp.z=Z;vp=mul(vp,tW);uv=mul(uv,tTex);}

technique Clamp{
	pass P0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;MipFilter[0]=LINEAR;MinFilter[0]=LINEAR;MagFilter[0]=LINEAR;
    VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();}
	pass P1{AddressU[0]=CLAMP;AddressV[0]=CLAMP;MipFilter[0]=POINT;MinFilter[0]=POINT;MagFilter[0]=POINT;
    VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();}
}
technique Wrap{
	pass P0{AddressU[0]=WRAP;AddressV[0]=WRAP;MipFilter[0]=LINEAR;MinFilter[0]=LINEAR;MagFilter[0]=LINEAR;
    VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();}
	pass P1{AddressU[0]=WRAP;AddressV[0]=WRAP;MipFilter[0]=POINT;MinFilter[0]=POINT;MagFilter[0]=POINT;
    VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();}
}
technique Mirror{
	pass P0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;MipFilter[0]=LINEAR;MinFilter[0]=LINEAR;MagFilter[0]=LINEAR;
    VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();}
	pass P1{AddressU[0]=MIRROR;AddressV[0]=MIRROR;MipFilter[0]=POINT;MinFilter[0]=POINT;MagFilter[0]=POINT;
    VertexShader=compile vs_2_0 VS();PixelShader=compile ps_2_0 PS();}

}
//technique Clamp{pass pp0{AddressU[0]=CLAMP;AddressV[0]=CLAMP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
//technique Wrap{pass pp0{AddressU[0]=WRAP;AddressV[0]=WRAP;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
//technique Mirror{pass pp0{AddressU[0]=MIRROR;AddressV[0]=MIRROR;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
//technique Border{pass pp0{AddressU[0]=BORDER;AddressV[0]=BORDER;BorderColor[0]=BorderCol;vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}