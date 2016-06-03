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
			float4 scale;
		};

        void vert (inout appdata_full v, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
			OUT.normal = v.normal;
			OUT.scale = mul (_Object2World, float3(1, 1, 1));
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

			if (IN.normal.y != 0) {
				// The normal points up, which means we're drawing top _and_ bottom
				if (IN.uv_MainTex.y >= _MainTex_TexelSize.y) {
					// calculate uv for bumpmap. The texture y coords are divided by 4
					// from the code to reduce texture bleed. So we multiply up again here.
					float2 uv_bumpmap = IN.uv_BumpMap;
					uv_bumpmap.y *= 4 * IN.scale.z;
					o.Normal = UnpackNormal (tex2D (_BumpMap, uv_bumpmap));

					float2 uv_lineBelow = float2(IN.uv_MainTex.x, IN.uv_MainTex.y - _MainTex_TexelSize.y);
					fixed4 cBelow = tex2D (_MainTex, uv_lineBelow);

					bool bottomFaceIsTransparent = c.a < 1;
					bool topFaceOnLineBelowIsTransparent = cBelow.a < 1;

					// TODO: check from script if the following condition holds for the whole
					// quad. If thats the case, skip creating it.
					if (bottomFaceIsTransparent == topFaceOnLineBelowIsTransparent) {
						o.Alpha = 0;
		         		return;
					}

					if (bottomFaceIsTransparent) {
						// Draw top face on line below instead
						c = cBelow;
						o.Normal *= -1;
					}
				}
			} else {
				o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));
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
