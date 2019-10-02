

float sqr(float x) { return x*x; }
//______________________________________________________________________________

float3 fresnel(float3 f0, float NdotV, float f90)
{		
	return f0 + (f90-f0) * pow(1.0 - max(0,NdotV), 5.0);
}
//______________________________________________________________________________

float modDisney(float NdotV, float NdotL, float LdotH, float linearRoughness)
{	
		
	float energyBias = lerp(0, 0.5f,  linearRoughness);
	float energyFactor = lerp(1.0, 1.0f / 1.51f,  linearRoughness);					
	float fd90 = energyBias + 2.0 * LdotH*LdotH * linearRoughness;
	float3 f0 = 1.0f;	
	float lightScatter = fresnel(f0.x,NdotL,fd90).x;
	float viewScatter = fresnel(f0.x,NdotV,fd90).x;
	
	return lightScatter * viewScatter * energyFactor;
}
//______________________________________________________________________________

float getAngleAtt(float3 p, float3 planeNormal, float2 angles)
{
	
	float cosInner = cos(angles.y);
	float cosOuter = cos(angles.x);
	float lightAngleScale = 1.0f/max(0.001f, angles.y);
	float lightAngleOffset = -cosOuter*lightAngleScale;
	float cd = dot(-planeNormal, normalize(p));
	float attenuation = saturate(cd * lightAngleScale + lightAngleOffset);
	attenuation *= attenuation;
	
	return attenuation;
}
//______________________________________________________________________________

float3 map(float3 x){

	float A = 0.15;
	float B = 0.50;
	float C = 0.10;
	float D = 0.20;
	float E = 0.02;
	float F = 0.3;
	
	return ((x*(A*x+C*B)+D*E)/(x*(A*x+B)+D*F))-E/F;
}
//______________________________________________________________________________

float3 ToneMapper(float3 col){
	
    col.rgb *= 6;	
	float W = 11.2;
	float ExposureBias = 2.0f;
	float3 curr = map(ExposureBias*col.rgb);
	float3 whiteScale = 1.0f/map(W);
	float3 color = curr*whiteScale;	
	col.rgb = pow(abs(color.rgb),rcp(2.2f));
	
	return col;
}
//______________________________________________________________________________

float3 GGX(SurfaceProp p, LightBuffer l)
{
	
	float3 lDirV;
	
	if(l.type == 0){
		lDirV = mul(l.pos,(float3x3)tLAV).xyz;	
	}else{
		lDirV = mul(float4(l.pos,1.0f), tLAV).xyz - p.vDirV;		
	}
	
	float3 n = p.tbn[2];
	float3 lv = normalize(lDirV);
	float NoL = saturate(dot(n,lv));
	
	if(NoL < 0.0){
		return 0;		
	}
	
	float3 t = p.tbn[0];
	float3 b = p.tbn[1];
	float3 v = -normalize(p.vDirV);
	float rough = p.mat.y;
	float metal = p.mat.x;
	
	float a = max(0.001f,sqr(rough));
	float3 H = normalize(lv + v);
	
	float VoN = abs(dot(v,n))+1e-5f;	
	float HoN = saturate(dot(H,n));	
	float LoH = saturate(dot(H,lv));
	float D;

	// ISOTROPIC/ANISOTROPIC NDF
	if(p.iso){				
		D = sqr(a) / pow( HoN * HoN * ( sqr(a) - 1.0 ) + 1.0, 2.0 );		
		}else{				
			float2 a2 = a*(1.0 - p.mat.zw * 0.9);			
			float HoT = dot(H,t);
			float HoB = dot(H,b);	
			float denom = sqr( HoT / a2.x) + sqr( HoB / a2.y) + sqr(HoN);	
			D = (1.0f / (a2.x*a2.y) ) * (1.0f / sqr(denom));		
	}

	// Shadowing/Visbility Term
	//float k = a * 0.5f;
	float k = sqr(0.5 + rough * 0.5)*0.5; // "Hotness" Remapping (Disney)
	float G_V = VoN + sqrt( (VoN - VoN * k) * VoN + k );
	float G_L = NoL + sqrt( (NoL - NoL * k) * NoL + k );
	float G = rcp( G_V * G_L );

	float f90 = 1;
	float3 f0 = 0.04f;  //fixed IOR for dielectric materials
	float D_mod = 1;
	
	if(disney){
		D_mod = modDisney(max(0,VoN), NoL, LoH, rough);
	}
	
	// to linear color space
	l.col = pow(abs(l.col),2.2)*l.lum; 
	
	// Dielectric vs Metallic
	f0 = lerp(f0,p.albedo,metal);
	p.albedo = lerp(p.albedo,0,metal);
	
	// Fresnel Term
	float3 F = fresnel(f0,LoH,f90);	
	float3 specular = D*G*F;	
   	float3 diffuse = p.albedo*D_mod; 
	float att = 1;
	
	if(l.type > 0){
		float sqrDist = max(dot(lDirV, lDirV),0.01f*0.01f);
		att = sqr(saturate(1.0 - pow(length(lDirV)/l.rad,4)))/sqrDist;
	} 
	
	if(l.type == 2){	
		float3 dir = normalize(mul(l.dir,(float3x3)tLAV)).xyz;	
		float2 lAng = float2(radians(floor(l.ang)),frac(l.ang))*0.5;
		att *= getAngleAtt(lDirV, dir, lAng);
	}			
	
	return (specular + diffuse) * NoL * l.col * att; 
}