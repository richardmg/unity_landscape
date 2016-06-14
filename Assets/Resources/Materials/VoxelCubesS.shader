Shader "Custom/VoxelCubesS"
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
		}

		LOD 100

		Pass
		{

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
				float4 objVertex : POSITION1;
				float3 normal : NORMAL;
				float4 extra : COLOR1;
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
				o.objVertex = v.vertex;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;
				o.extra.x = length(mul(_Object2World, float3(0, 0, 1))); // z scale

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// NB: OpenGL has XY at lower left, which will be reflected in the vars
				float lightMax = 0.5;
				float lightDampning = 0.02;
				float light = 1;

				fixed4 red = fixed4(1, 0, 0, 1);
				float2 textureSize = float2(_TextureWidth, _TextureHeight);
				float2 subImageSize = float2(_SubImageWidth, _SubImageHeight);

				float2 atlasPixel = i.uv * textureSize;
				// We can get requests for pixels outside the vertices. But this will cause seams to
				// show when sampling from a texture atlas. So ensure that we always sample from
				// within the subimage.
				if (i.objVertex.x < -0.5f)
					atlasPixel.x = floor(atlasPixel.x - i.objVertex.x + 0.4f);
				else if (i.objVertex.x > subImageSize.x - 0.6f)
					atlasPixel.x = atlasPixel.x - (i.objVertex.x - (subImageSize.x - 0.6f));
				if (i.objVertex.y < -0.5f)
					atlasPixel.y = floor(atlasPixel.y - i.objVertex.y + 0.4f);
				else if (i.objVertex.y > subImageSize.y - 0.6f)
					atlasPixel.y = atlasPixel.y - (i.objVertex.y - (subImageSize.y - 0.6f));

				float2 subImagePixel = atlasPixel % subImageSize;
				float2 atlasPixelInt = floor(atlasPixel);
				float2 subImagePixelInt = floor(subImagePixel);
				float2 atlasIndex = floor(atlasPixel / subImageSize);
				float2 atlasSubImageBottomLeft = atlasIndex * subImageSize;

				float2 uvOnePixel = 1.0f / textureSize;
				float2 uvInsideVoxel = frac(atlasPixel);
				float2 uvAtlasVoxelCenter = (atlasPixelInt + 0.5) * uvOnePixel;

				fixed4 c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter, 0, 0));

				if (c.a == 0) {
					// Always sample opaque pixels. Transparent pixels are not converted to voxels, but
					// will still bleed in from the edges when drawing opaque voxels unless we do this clamping.
					float seam = 0.5f;
					float oneMinusSeam = 1 - seam;
					bool leftEdge = uvInsideVoxel.x < seam && subImagePixelInt.x > 0;
					bool rightEdge = uvInsideVoxel.x > oneMinusSeam && subImagePixelInt.x < subImageSize.x - 1;
					bool topEdge = uvInsideVoxel.y > oneMinusSeam && subImagePixelInt.y < subImageSize.y - 1;
					bool bottomEdge = uvInsideVoxel.y < seam && subImagePixelInt.y > 0;

					if (leftEdge)
						c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter.x - uvOnePixel.x, uvAtlasVoxelCenter.y, 0, 0));
					else if (rightEdge)
						c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter.x + uvOnePixel.x, uvAtlasVoxelCenter.y, 0, 0));

					if (c.a == 0) {
						if (bottomEdge)
							c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter.x, uvAtlasVoxelCenter.y - uvOnePixel.y, 0, 0));
						else if (topEdge)
							c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter.x, uvAtlasVoxelCenter.y + uvOnePixel.y, 0, 0));
					}

					if (c.a == 0) {
						// Check corners
						if (leftEdge) {
							if (bottomEdge)
								c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter.x - uvOnePixel.x, uvAtlasVoxelCenter.y - uvOnePixel.y, 0, 0));
							else if (topEdge)
								c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter.x - uvOnePixel.x, uvAtlasVoxelCenter.y + uvOnePixel.y, 0, 0));
						} else if (rightEdge) {
							if (bottomEdge)
								c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter.x + uvOnePixel.x, uvAtlasVoxelCenter.y - uvOnePixel.y, 0, 0));
							else if (topEdge)
								c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter.x + uvOnePixel.x, uvAtlasVoxelCenter.y + uvOnePixel.y, 0, 0));
						}
					}
				}

				if (i.normal.x != 0) {
				} else if (i.normal.y != 0) {
				} else {
				}

#ifdef DEBUG_TEXTURE_ATLAS
				if (c.a != 1 && c.a != 0)
					c = red;
#endif

#ifdef USE_LIGHT
				c *= light;
//				c = DiffuseLight( i.lightDir, normal, c, LIGHT_ATTENUATION(i) );
#endif
				return c;
			}
			ENDCG
		}
	}
}
