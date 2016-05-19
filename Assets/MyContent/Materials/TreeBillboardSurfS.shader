Shader "Custom/TreeBillboardSurfS" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Cutoff("Cutoff", Range(0,1)) = 0.5
	}
	SubShader {
	    Tags { "RenderType" = "Transparent"}
	    Blend SrcAlpha OneMinusSrcAlpha
	    Cull off
		
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows alphatest:_Cutoff
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

			o.Albedo = c.rgb;
			o.Alpha = (c.r == 0 && c.g == 0 && c.b == 0) ? 0 : 1;
//
//			// Metallic and smoothness come from slider variables
//			o.Metallic = _Metallic;
//			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
