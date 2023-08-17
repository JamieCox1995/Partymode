Shader "Interactable/Highlighted"
{
	Properties
	{
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_MainTex ("Main Texture", 2D) = "white" {}
		[HDR]_OutlineColor("Highlight Color", Color) = (1, 1, 1, 1)
		_HighlightWidth("Highlight Width", Range(0.05, 5.0)) = 1.0
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float4 pos : POSITION;
	};

	float _HighlightWidth;
	float4 _OutlineColor;

	v2f vert(appdata v)
	{
		v.vertex.xyz *= _HighlightWidth;

		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);

		return o;
	}

	ENDCG

	SubShader
	{
		Tags {"RenderType" = "Transparent"}

		// Rendering the object outline
		Pass
		{

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert 
			#pragma fragment frag

			float4 frag(v2f i) : COLOR
			{
				float4 col = _OutlineColor;

				//col *= 3;

				return col;
			}

			ENDCG
		}

		// Setting the Standard Shader settings
		Pass
		{
			ZWrite On

			Material
			{
				Diffuse[_Color]
				Ambient[_Color]
			}

			Lighting On
			
			SetTexture[_MainTex]
			{
				ConstantColor[_Color]
			}

			SetTexture[_MainTex]
			{
				Combine previous * primary DOUBLE
			}
		}
	}

	Fallback "Standard"
}