float2 R;
bool Grayscale=true;
texture tex0;
sampler s0=sampler_state{Texture=(tex0);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
texture tex1;
sampler s1=sampler_state{Texture=(tex1);MipFilter=LINEAR;MinFilter=LINEAR;MagFilter=LINEAR;};
float mx(float3 p){return max(p.x,max(p.y,p.z));}
float4 p0(float2 x:TEXCOORD0):color{
    float2 vp=x*R-.25;
    float2 sz=float2(8,12);
    float4 c=tex2D(s0,floor(vp/sz)*sz/R+.5/R);
    float grey=mx(c.rgb);
    //grey=dot(c.rgb,1.)/3.;
    grey=pow(grey,5.);
	float letter=tex2D(s1,(frac(vp/sz)+float2(grey*176,0))/float2(176,1));
    c.rgb=normalize(c.rgb)*sqrt(3)*letter;
    if(Grayscale)c.rgb=letter;
	return c;
}
void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Ascii{pass pp0{vertexshader=compile vs_2_0 vs2d();pixelshader=compile ps_2_0 p0();}}
