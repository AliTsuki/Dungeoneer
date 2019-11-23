﻿Shader "Ali/CameraMaskEffectShader"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_MaskTex("Mask Tex", 2D) = "white" {}
		_FogTex("Fog Tex", 2D) = "white" {}
		_FogIntensity("Fog Intensity", float) = 0.5
	}

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _MaskTex;
			sampler2D _FogTex;
			float _FogIntensity;

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				float4 fog = tex2D(_FogTex, i.uv);
				fog *= _FogIntensity;
				float4 mask = tex2D(_MaskTex, i.uv);
				col *= fog;
				col *= mask;
				return col;
			}
		ENDCG
		}
	}
}
