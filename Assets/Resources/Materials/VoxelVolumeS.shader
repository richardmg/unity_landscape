Shader "Custom/VoxelVolume" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.0
		_Metallic ("Metallic", Range(0,1)) = 0.8
      	_CutOff("Cut off", Range(0,1)) = 1.0
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
		#pragma surface surf Standard alphatest:_CutOff vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		float4 _MainTex_TexelSize;
		half _Glossiness;
		half _Metallic;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 normal;
//			float3 worldNormal;
//			INTERNAL_DATA
		};

        void vert (inout appdata_full v, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
			OUT.normal = v.normal;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			bool isTransparent = c.a < 1;

			if (IN.normal.y == 1) {
				// The normal points up, which means we're drawing top _and_ bottom
				if (IN.uv_MainTex.y >= _MainTex_TexelSize.y) {
					float2 uv_lineBelow = float2(IN.uv_MainTex.x, IN.uv_MainTex.y - _MainTex_TexelSize.y);
					fixed4 cBelow = tex2D (_MainTex, uv_lineBelow);
					bool belowIsTransparent = cBelow.a < 1;

					// TODO: check from script if the following condition holds for the whole
					// quad. If thats the case, skip creating it.
					if (isTransparent == belowIsTransparent) {
						o.Alpha = 0;
		         		return;
					}

					if (isTransparent)
						c = cBelow;
//					o.Normal = IN.worldNormal;
//					o.Normal = WorldNormalVector (IN, o.Normal);
				}
			} else {
//				o.Normal = IN.worldNormal;
//				o.Normal = WorldNormalVector (IN, o.Normal);
//				o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
//				c = fixed4(1, 0, 0, 1);
			}

			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
