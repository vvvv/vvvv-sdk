float2 R;
float2 R2;
float2 Amount=(1,1);
float2 AmountPRE=(1,1);
float BlurWidth <float uimin=0;> =1;
texture tex0,map,mapPRE;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s1=sampler_state{Texture=(map);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
sampler s2=sampler_state{Texture=(mapPRE);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};

float4 p0(float2 x:TEXCOORD0,float2 x2:TEXCOORD1):color{
	float4 c=0;
	for (float i=0;i<1;i+=1./12.){
		c+=tex2D(s0,lerp(x,lerp(tex2D(s1,x2),tex2D(s2,x2),(i-.5)*BlurWidth+.5).xy,lerp(Amount,AmountPRE,(i-.5)*BlurWidth+.5)));
	}
	c/=12.;
    return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0,out float2 uv2:TEXCOORD1){vp.xy*=2;uv2=uv+.5/R2;uv+=.5/R;}
technique DisplaceMB{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
