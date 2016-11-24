Shader "Custom/VoxelObjectSurfaceShader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MainTex2 ("Albedo (RGB)", 2D) = "white" {}
		_MainTex3 ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap ("Normal map", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM

		#include "VoxelObjectCommon.cginc"

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv2_MainTex2;
			float2 uv3_MainTex3;
//			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float2 uvAtlasClamped = uvClamped(IN.uv_MainTex, IN.uv2_MainTex2);
			fixed4 c = tex2D(_MainTex, uvAtlasClamped);

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv3_MainTex3));
//
//			if (IN.uv3_MainTex3.y > 3)
//				o.Albedo.rgb = float3(1, 0, 0);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
