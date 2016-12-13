
float2 FilterOffset : FILTEROFFSET;

texture BackBuffer;// : BACKBUFFER;


sampler BackBufferSampler = 
sampler_state
{
    Texture = <BackBuffer>;
    MinFilter = LINEAR;  
    MagFilter = LINEAR;
    MipFilter = None;

    AddressU = Clamp;
    AddressV = Clamp;
};

struct VS_OUTPUT
{
    float4 Position  : POSITION ;
   	float2 Uv : TEXCOORD0 ;
    
};



VS_OUTPUT VS_Fullscreen(float4 Position : POSITION, float2 Uv : TEXCOORD0)
{
 	VS_OUTPUT Out = (VS_OUTPUT)0;
 	Out.Position = float4(Position.xy,0,1)*float4(2,2,1,1);
	Out.Uv = Uv;
	
	return Out;    
}


float4 PS_FXAAPreProcess(VS_OUTPUT In) : COLOR0
{
	float2 uv = In.Uv + FilterOffset * 0.5f;
	float4 texVal = tex2D(BackBufferSampler, uv);
	texVal.w = dot(texVal.xyz, float3(0.299, 0.587, 0.114)); // compute luma
	
	return texVal;
}



float4 FxaaTexOff(sampler2D t, float2 p, float2 o, float2 r)
{
	return tex2Dlod(t, float4(p + (o * r), 0, 0));	
}



float4 PS_FXAA(VS_OUTPUT In) : COLOR0
{
	float2 pos = In.Uv + FilterOffset * 0.5f;
	 
    float lumaN = FxaaTexOff(BackBufferSampler, pos.xy, float2(0, -1), FilterOffset).w;
    float lumaW = FxaaTexOff(BackBufferSampler, pos.xy, float2(-1, 0), FilterOffset).w;
    float4 rgbyM = tex2Dlod(BackBufferSampler, float4(pos.xy,0,0));
    float lumaE = FxaaTexOff(BackBufferSampler, pos.xy, float2( 1, 0), FilterOffset).w;
    float lumaS = FxaaTexOff(BackBufferSampler, pos.xy, float2( 0, 1), FilterOffset).w;
    float lumaM = rgbyM.w;
 
    float rangeMin = min(lumaM, min(min(lumaN, lumaW), min(lumaS, lumaE)));
    float rangeMax = max(lumaM, max(max(lumaN, lumaW), max(lumaS, lumaE)));
    float range = rangeMax - rangeMin;
    if(range < max(0.0833f, rangeMax * 0.1667))
    {
    	clip(-1);
    }
        
    float lumaNW = FxaaTexOff(BackBufferSampler, pos.xy, float2(-1,-1), FilterOffset).w;
    float lumaNE = FxaaTexOff(BackBufferSampler, pos.xy, float2( 1,-1), FilterOffset).w;
    float lumaSW = FxaaTexOff(BackBufferSampler, pos.xy, float2(-1, 1), FilterOffset).w;
    float lumaSE = FxaaTexOff(BackBufferSampler, pos.xy, float2( 1, 1), FilterOffset).w;

    float lumaL = (lumaN + lumaW + lumaE + lumaS) * 0.25;
    float rangeL = abs(lumaL - lumaM);
    float blendL = saturate((rangeL / range) - 0.25f) * 1.333f; 
    blendL = min(0.75f, blendL);

    float edgeVert = 
              abs(lumaNW + (-2.0 * lumaN) + lumaNE) +
        2.0 * abs(lumaW  + (-2.0 * lumaM) + lumaE ) +
              abs(lumaSW + (-2.0 * lumaS) + lumaSE);
    float edgeHorz = 
              abs(lumaNW + (-2.0 * lumaW) + lumaSW) +
        2.0 * abs(lumaN  + (-2.0 * lumaM) + lumaS ) +
              abs(lumaNE + (-2.0 * lumaE) + lumaSE);
    bool horzSpan = edgeHorz >= edgeVert;

    float lengthSign = horzSpan ? -FilterOffset.y : -FilterOffset.x;
    if(!horzSpan)
    {	
    	lumaN = lumaW;
    }
    if(!horzSpan) 
    {	
    	lumaS = lumaE;
    }
    float gradientN = abs(lumaN - lumaM);
    float gradientS = abs(lumaS - lumaM);
    lumaN = (lumaN + lumaM) * 0.5;
    lumaS = (lumaS + lumaM) * 0.5;

    bool pairN = gradientN >= gradientS;
    if(!pairN) 
    {
    	lumaN = lumaS;
    }
    if(!pairN) 
    {
    	gradientN = gradientS;
    }
    if(!pairN) 
    {
    	lengthSign *= -1.0;
    }
    float2 posN;
    posN.x = pos.x + (horzSpan ? 0.0 : lengthSign * 0.5);
    posN.y = pos.y + (horzSpan ? lengthSign * 0.5 : 0.0);

    gradientN *= 0.25f;

    float2 posP = posN;
    float2 offNP = horzSpan ? float2(FilterOffset.x, 0.0) : float2(0.0f, FilterOffset.y); 
    float lumaEndN;
    float lumaEndP;
    bool doneN = false;
    bool doneP = false;
    posN += offNP * (-1.5);
    posP += offNP * ( 1.5);
    int i = 0;
    while(i < 6)
    {
        lumaEndN = tex2Dlod(BackBufferSampler, float4(posN.xy,0,0)).w;
        lumaEndP = tex2Dlod(BackBufferSampler, float4(posP.xy,0,0)).w;
        bool doneN2 = abs(lumaEndN - lumaN) >= gradientN;
        bool doneP2 = abs(lumaEndP - lumaN) >= gradientN;
        if(doneN2 && !doneN) 
        {
        	posN += offNP;
        }
        if(doneP2 && !doneP) 
        {
        	posP -= offNP;
        }
        if(doneN2 && doneP2) 
        {
        	i = 6;
        }
        doneN = doneN2;
        doneP = doneP2;
        if(!doneN) 
        {	
        	posN -= offNP * 2.0;
        }
        if(!doneP) 
        {
        	posP += offNP * 2.0; 
        }
        ++i;
    }

    float dstN = horzSpan ? pos.x - posN.x : pos.y - posN.y;
    float dstP = horzSpan ? posP.x - pos.x : posP.y - pos.y;

    bool directionN = dstN < dstP;
    lumaEndN = directionN ? lumaEndN : lumaEndP;

    if(((lumaM - lumaN) < 0.0) == ((lumaEndN - lumaN) < 0.0)) 
    {
        lengthSign = 0.0;
    }

    float spanLength = (dstP + dstN);
    dstN = directionN ? dstN : dstP;
    float subPixelOffset = 0.5 + (dstN * (-1.0/spanLength));
    subPixelOffset += blendL * (1.0/8.0);
    subPixelOffset *= lengthSign;
    float3 rgbF = tex2D(BackBufferSampler, float2(
        pos.x + (horzSpan ? 0.0 : subPixelOffset),
        pos.y + (horzSpan ? subPixelOffset : 0.0))).xyz;

    float lumaF = dot(rgbF, float3(0.299, 0.587, 0.114)) + (1.0/(65536.0*256.0));
    float lumaB = lerp(lumaF, lumaL, blendL);
    float scale = min(4.0, lumaB/lumaF);
    rgbF *= scale;
    return float4(rgbF,1);//return float4(rgbF, lumaM); 
}




technique FXAAPreProcess
{
    pass FXAA3
    {   
        VertexShader = compile vs_3_0 VS_Fullscreen();
        PixelShader = compile ps_3_0 PS_FXAAPreProcess();
		AlphaBlendEnable = false;	
		AlphaTestEnable = false;		
		ZEnable = false;
		ZFunc = Always;
		ZWriteEnable = false;
		FillMode = solid;
		CullMode = None;
		StencilEnable = false;
		ColorWriteEnable = Red|Green|Blue|Alpha;
    }
};

technique FXAA
{
    pass FXAA3
    {   
        VertexShader = compile vs_3_0 VS_Fullscreen();
        PixelShader = compile ps_3_0 PS_FXAA();
		AlphaBlendEnable = false;	
		AlphaTestEnable = false;		
		ZEnable = false;
		ZFunc = Always;
		ZWriteEnable = false;
		FillMode = solid;
		CullMode = None;
		StencilEnable = false;
		ColorWriteEnable = Red|Green|Blue|Alpha;
    }
};
