Shader "Custom/VoxelVolumeUnlitS"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TextureWidth ("Texture width", Int) = 64
		_TextureHeight ("Texture height", Int) = 64
		_SubImageWidth ("Subimage width", Int) = 16
		_SubImageHeight ("Subimage height", Int) = 8
	}
	SubShader
	{
		Tags {
			"RenderType"="Opaque"
			"Queue" = "AlphaTest"
		}

		LOD 100

		Pass
		{
      	 	Cull Off

			CGPROGRAM

			#define USE_LIGHT
			#define DEBUG_TEXTURE_ATLAS

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 extra : COLOR0; // x: left or right edge, y: top or bottom edge, z: z scale
			};


			int _TextureWidth;
			int _TextureHeight;
			int _SubImageWidth;
			int _SubImageHeight;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;

				o.extra.x = v.vertex.x;
				o.extra.y = v.vertex.y;
				o.extra.z = length(mul(_Object2World, float3(0, 0, 1)));

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float lightMax = 0.5;
				float lightDampning = 0.02;
				float light = 1;
				float uvOnePixelX = (1.0 / _TextureWidth);
				float uvOnePixelY = (1.0 / _TextureHeight);
				fixed4 c = tex2D(_MainTex, i.uv);

				if (i.normal.x != 0) {
					// Columns (left to right)
					if (i.extra.x == 0) {
						// Left edge
						light = 1 + lightMax - (lightDampning * _SubImageWidth);
					} else if (i.extra.x == _SubImageWidth) {
						// Right edge
						light = 1 + lightMax;
					} else {
						// Center edges
						float2 uv_lineLeft = float2(i.uv.x - uvOnePixelX, i.uv.y);
						fixed4 cLeft = tex2D (_MainTex, uv_lineLeft);

						bool leftFaceIsTransparent = c.a == 0;
						bool rightFaceOnLineLeftIsTransparent = cLeft.a == 0;

						if (leftFaceIsTransparent == rightFaceOnLineLeftIsTransparent) {
							discard;
			         		return c;
						}

						if (leftFaceIsTransparent) {
							// Draw right face on line left instead
							c = cLeft;
							light = 1 + lightMax - (lightDampning * (_SubImageWidth - i.extra.x));
						} else {
							light = 1 + lightMax - (lightDampning * (_SubImageWidth - i.extra.x + 10));
						}
					}
				} else if (i.normal.y != 0) {
					// Rows (bottom to top)
					if (i.extra.y == 0) {
						// Bottom edge
						light = 1 + lightMax - (lightDampning * _SubImageHeight);
					} else if (i.extra.y == _SubImageHeight) {
						// Top edge
						light = 1 + lightMax;
					} else {
						// Center edges
						float2 uv_lineBelow = float2(i.uv.x, i.uv.y - uvOnePixelY);
						fixed4 cBelow = tex2D (_MainTex, uv_lineBelow);

						bool bottomFaceIsTransparent = c.a == 0;
						bool topFaceOnLineBelowIsTransparent = cBelow.a == 0;

						if (bottomFaceIsTransparent == topFaceOnLineBelowIsTransparent) {
							discard;
			         		return c;
						}

						if (bottomFaceIsTransparent) {
							// Draw top face on line below instead
							c = cBelow;
							light = 1 + lightMax - (lightDampning * (_SubImageHeight - i.extra.y));
						} else {
							light = 1 + lightMax - (lightDampning * (_SubImageHeight - i.extra.y + 10));
						}
					}
				} else {
					// Front and back
					if (i.normal.z == 1)
						light = 1 + lightMax - (lightDampning * (_SubImageHeight + 11));
				}

				if (c.a < 0.5)
					discard;

#ifdef DEBUG_TEXTURE_ATLAS
				if (c.a != 1 && c.a != 0)
					c = fixed4(1, 0, 0, 1);
#endif

#ifdef USE_LIGHT
				c *= light;
#endif
				return c;
			}
			ENDCG
		}
	}
}
