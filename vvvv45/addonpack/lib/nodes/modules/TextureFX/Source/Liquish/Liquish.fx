float2 R;
float time;
//float4 ColorA:COLOR;
float4 ColorB:COLOR = {1, 0, 0, 1};
float4 ColorC:COLOR = {0, 1, 0, 1};
float4 ColorD:COLOR = {0, 0, 1, 1};

//Ported from GLSL
// all credit to :
//http://glsl.heroku.com/e#1252.0    @SyntopiaDK, 2012

float rand(float2 co){
	// implementation found at: lumina.sourceforge.net/Tutorials/Noise.html
	return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
}

float noise2f( in float2 p )
{
	float2 ip = float2(floor(p));
	float2 u = frac(p);
	// http://www.iquilezles.org/www/articles/morenoise/morenoise.htm
	u = u*u*(3.0-2.0*u);
	//u = u*u*u*((6.0*u-15.0)*u+10.0);	
	float res = lerp(
		lerp(rand(ip),  rand(ip+float2(1.0,0.0)),u.x),
		lerp(rand(ip+float2(0.0,1.0)),   rand(ip+float2(1.0,1.0)),u.x),
		u.y);
	return res*res;
}

float fbm(float2 c) {
	float f = 0.0;
	float w = 1.0;
	for (int i = 0; i < 4; i++) {
		f+= w*noise2f(c);
		c*=2.0;
		w*= 0.5;
	}
	return f;
}

float2 cMul(float2 a, float2 b) {
	return float2( a.x*b.x -  a.y*b.y,a.x*b.y + a.y * b.x);
}

float pattern(  float2 p, out float2 q, out float2 r ) {
	q.x = fbm( p  +0.00*time);
	q.y = fbm( p + float2(1.0, 1.0));
	
	r.x = fbm( p +1.0*q + float2(1.7,9.2)+0.15*time );
	r.y = fbm( p+ 1.0*q + float2(8.3,2.8)+0.126*time);
	//r = cMul(q,q+0.1*time);
	return fbm(p +1.0*r + 0.0* time);
}

float4 p0(float2 vp:vpos):color  {
	float2 q;
	float2 r;
	float2 c = 1000.0 * vp.xy / R.xy;
	float f = pattern(c*0.01,q,r);
	
	float3 colors[3];
	//colors[0] = float3 (ColorA.r, ColorA.g, ColorA.b);
	colors[0] = float3 (ColorB.r, ColorB.g, ColorB.b);
	colors[1] = float3 (ColorC.r, ColorC.g, ColorC.b);
	colors[2] = float3 (ColorD.r, ColorD.g, ColorD.b);
	
	float3 col = lerp(colors[0],colors[1],clamp((f*f)*4.0,0.0,1.0));
	//col = colors[1];
	col = lerp(col,colors[1],clamp(length(q),0.0,1.0));
	col = lerp(col,colors[2],clamp(length(r.x),0.0,1.0));
	
	float alphas[3];
	//alphas[0] = float (ColorA.a);
	alphas[0] = float (ColorB.a);
	alphas[1] = float (ColorC.a);
	alphas[2] = float (ColorD.a);
	
	//float alph = lerp(alphas[0],alphas[1],clamp((f)*4.0,0.0,1.0));
	float alph = lerp(alphas[0],alphas[1],clamp((f)*4.0,0.0,1.0));
	alph = lerp(alph,alphas[1],clamp(length(q),0.0,1.0));
	alph = lerp(alph,alphas[2],clamp(length(r.x),0.0,1.0));
	
	return  float4((0.2*f*f*f+0.6*f*f+0.5*f)*atan(col), alph);
}

void vs2d(inout float4 vp:POSITION0,inout float2 uv:TEXCOORD0){vp.xy*=2;uv+=.5/R;}
technique Liquish{pass pp0{vertexshader=compile vs_3_0 vs2d();pixelshader=compile ps_3_0 p0();}}
