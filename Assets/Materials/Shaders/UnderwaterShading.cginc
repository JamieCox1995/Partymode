#if !defined (UNDERWATER_SHADING)
#define UNDERWATER_SHADING

sampler2D _CameraDepthTexture, _WaterBackground;
float4 _CameraDepthTexture_TexelSize;
float3 _WaterFogColor;
float _WaterFogDensity, _WaterFoamThickness;

float3 UnderwaterShading(float4 screenPos)
{
	float2 uv = screenPos.xy / screenPos.w;

	float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
	float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);

	float difference = backgroundDepth - surfaceDepth;

	float3 backgroundColor = tex2D(_WaterBackground, uv).rgb;

	float fog = exp2(-_WaterFogDensity * difference);

	return lerp(_WaterFogColor, backgroundColor, fog);
}

float4 WaterEdgeShading(float4 screenPos)
{
	float2 uv = screenPos.xy / screenPos.w;
	float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));

	float4 color = 1 - saturate(_WaterFoamThickness * (depth - screenPos.w));
	//float4 color = 1 - (difference / _WaterFoamThickness) + 1.0;

	return color;
}

#endif