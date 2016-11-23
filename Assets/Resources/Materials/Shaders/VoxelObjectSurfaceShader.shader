Shader "Custom/VoxelObjectSurfaceShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MainTex2 ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM

		#include "TestFunctions.cginc"

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv2_MainTex2;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		static float2 _TextureSize = float2(2048, 2048);
		static float2 _SubImageSize = float2(16, 16);
		static float2 _UVAtlasOnePixel = 1.0f / _TextureSize;
		static float2 _UVAtlasHalfPixel = _UVAtlasOnePixel / 2;
		static float _ClampOffset = 0.00001;

		inline float2 uvClamped(float2 uvAtlas, float2 uvPixel)
		{
			float diffX = uvAtlas.x - uvPixel.x;
			float diffY = uvAtlas.y - uvPixel.y;
			float2 uvAtlasClamped = uvAtlas;
			uvAtlasClamped.x -= if_gt(diffX, _UVAtlasOnePixel.x - _ClampOffset) * _UVAtlasHalfPixel.x;
			uvAtlasClamped.y -= if_gt(diffY, _UVAtlasOnePixel.y - _ClampOffset) * _UVAtlasHalfPixel.y;
			uvAtlasClamped.x += if_lt(diffX, _ClampOffset) * _UVAtlasHalfPixel.x;
			uvAtlasClamped.y += if_lt(diffY, _ClampOffset) * _UVAtlasHalfPixel.y;
			return uvAtlasClamped;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float2 uvAtlasClamped = uvClamped(IN.uv_MainTex, IN.uv2_MainTex2);
			fixed4 c = tex2D (_MainTex, uvAtlasClamped);

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
