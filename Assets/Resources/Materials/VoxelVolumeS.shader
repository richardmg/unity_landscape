Shader "Custom/CutoffTwosidedLitS" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.0
		_Metallic ("Metallic", Range(0,1)) = 0.8
      	_CutOff("Cut off", Range(0,1)) = 0.8
	}
	SubShader {
		Tags {
			"RenderType" = "Opaque"
			"Queue" = "AlphaTest"
		}
		Cull Off

		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
	 	uniform float _CutOff; 

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldNormal;
			INTERNAL_DATA
		};

		half _Glossiness;
		half _Metallic;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
         	if (c.a < _CutOff)
         		discard;
			o.Albedo = c.rgb;

//			if (IN.worldNormal.z == -1)
				o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
