float2 R;
float4 fromBlack:COLOR=0;
float4 fromWhite:COLOR=1;
float4 toBlack:COLOR=0;
float4 toWhite:COLOR=1;
float4 Gamma=1;
bool ClampColor=0;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float4 p0(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
	c=((c-fromBlack)/(fromWhite-fromBlack));
    c=sign(c)*pow(abs(c),Gamma);
	//if(ClampColor)c=saturate(c);
	
	c=c*(toWhite-toBlack)+toBlack;
    return c;
}
float4 pCLAMP_TOP(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
	c=((c-fromBlack)/(fromWhite-fromBlack));
    c=sign(c)*pow(abs(c),Gamma);
	//if(ClampColor)c=saturate(c);
	c=min(c,1);
	c=c*(toWhite-toBlack)+toBlack;
    return c;
}
float4 pCLAMP_BOTTOM(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
	c=((c-fromBlack)/(fromWhite-fromBlack));
    c=sign(c)*pow(abs(c),Gamma);
	//if(ClampColor)c=saturate(c);
	c=max(c,0);
	c=c*(toWhite-toBlack)+toBlack;
    return c;
}
float4 pCLAMP_BOTH(float2 vp:vpos):color{float2 x=(vp+.5)/R;
    float4 c=tex2D(s0,x);
	c=((c-fromBlack)/(fromWhite-fromBlack));
    c=sign(c)*pow(abs(c),Gamma);
	//if(ClampColor)c=saturate(c);
	c=saturate(c);
	c=c*(toWhite-toBlack)+toBlack;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Levels{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
technique ClampBottom{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pCLAMP_BOTTOM();}}
technique ClampTop{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pCLAMP_TOP();}}
technique ClampBoth{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 pCLAMP_BOTH();}}


