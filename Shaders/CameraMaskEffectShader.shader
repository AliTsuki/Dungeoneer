Shader "Ali/CameraMaskEffectShader"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_MaskTex("Mask Tex", 2D) = "white" {}
		_FogOfWarTex("Fog of War Tex", 2D) = "white" {}
		_FogDarken("Fog Darken", Range(0, 1)) = 0.95
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
			sampler2D _FogOfWarTex;
			float _FogDarken;
			float4 _MainTex_TexelSize;

			float4 gaussianBlur(sampler2D tex, float2 uv, float4 size)
			{
				// Sample horizontally and vertically with decreasing weights increasing distance
				float4 col =
					// Center
					tex2D(tex, uv + float2(0, 0))
					// Left
					+ (tex2D(tex, uv + float2(-size.x,     0)))
					+ (tex2D(tex, uv + float2(-size.x * 2, 0)) * 0.75)
					+ (tex2D(tex, uv + float2(-size.x * 3, 0)) * 0.5)
					+ (tex2D(tex, uv + float2(-size.x * 4, 0)) * 0.25)
					// Right
					+ (tex2D(tex, uv + float2(size.x,     0)))
					+ (tex2D(tex, uv + float2(size.x * 2, 0)) * 0.75)
					+ (tex2D(tex, uv + float2(size.x * 3, 0)) * 0.5)
					+ (tex2D(tex, uv + float2(size.x * 4, 0)) * 0.25)
					// Down
					+ (tex2D(tex, uv + float2(0, -size.y    )))
					+ (tex2D(tex, uv + float2(0, -size.y * 2)) * 0.75)
					+ (tex2D(tex, uv + float2(0, -size.y * 3)) * 0.5)
					+ (tex2D(tex, uv + float2(0, -size.y * 4)) * 0.25)
					// Up
					+ (tex2D(tex, uv + float2(0, size.y    )))
					+ (tex2D(tex, uv + float2(0, size.y * 2)) * 0.75)
					+ (tex2D(tex, uv + float2(0, size.y * 3)) * 0.5);
				    + (tex2D(tex, uv + float2(0, size.y * 4)) * 0.25);
				// Weighted average
				col /= 11;
				return col;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// Sample render textures
				float4 col = tex2D(_MainTex, i.uv);
				float4 mask = (0, 0, 0, 0);
				float4 fog = tex2D(_FogOfWarTex, i.uv);
				// Blur mask
				mask = gaussianBlur(_MaskTex, i.uv, _MainTex_TexelSize);
				// Grayscale fog texture and darken
				fog.rgb = ((fog.r + fog.g + fog.b) / 3) * (1 - _FogDarken);
				// Invert mask and apply to fog
				fog *= (1 - mask);
				// Apply mask to color
				col *= mask;
				// Apply fog to color
				col += fog;
				// Return final color
				return col;
			}
		ENDCG
		}
	}
}
