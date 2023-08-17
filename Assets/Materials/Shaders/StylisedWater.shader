Shader "Partymode/Environment/Water" {
	Properties
	{
		_WaterSurfaceColor("Water Surface Color", Color) = (1,1,1,1)
		_WaterSurfaceTexture("Water Surface Texture, (RGB)", 2D) = "white" {}
		
		_WaterFogColor("Water Fog Color", Color) = (0,0,0,0)
		_WaterFogDensity("Water Fog Density", Range(0, 2)) = 0.1

		_WaterFoamColor("Water Foam Color", Color) = (1,1,1,1)
		_WaterFoamThickness("Water Foam Thickness", Range(0, 5)) = 0.5

		_Shininess("Shininess", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200

		// Grabbing the background and saving it to a texture names _WaterBackground
		GrabPass{"_WaterBackground"}

		CGPROGRAM
		// Physically based Standard lighting model
		#pragma surface surf Standard alpha finalcolor:ResetAlpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		// Including our custon CGINCLUDE file that has all of the functions for calculating the UnderwaterShading colors
		#include "UnderwaterShading.cginc"

		sampler2D _WaterSurfaceTexture;

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
		};

		// Declaring variables
		fixed4 _WaterSurfaceColor, _WaterFoamColor;
		float _Shininess;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 waterColor = tex2D(_WaterSurfaceTexture, IN.uv_MainTex) * _WaterSurfaceColor;		// Calculating the color of water. This is the actual water color
			fixed4 foam = WaterEdgeShading(IN.screenPos) * _WaterFoamColor;								// Calculating the color of the water edge

			o.Albedo = waterColor.rgb + foam.rgb;														// Adding the two colors together and setting the Albedo of the material
			o.Alpha = waterColor.a + foam.a;															// Adding the two colors together and setting the Alpha of the material

			o.Emission = UnderwaterShading(IN.screenPos) * (1 - waterColor.a);							// Applying the color of the fog.

			o.Smoothness = _Shininess;																	// Setting some Shininess
			//o.Smoothness = 1;
		}

		void ResetAlpha(Input IN, SurfaceOutputStandard o, inout fixed4 color)
		{
			color.a = 1;
		}

		ENDCG
	}
	//FallBack "Diffuse"
}
