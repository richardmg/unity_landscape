Shader "Custom/VoxelCubesS"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TextureWidth ("Texture width", Int) = 64
		_TextureHeight ("Texture height", Int) = 64
		_SubImageWidth ("Subimage width", Int) = 16
		_SubImageHeight ("Subimage height", Int) = 8
		_PixelateVoxelX ("Pixelate X", Range(0, 1)) = 0
		_PixelateVoxelY ("Pixelate Y", Range(0, 1)) = 0
		_PixelateVoxelZ ("Pixelate Z", Range(0, 1)) = 0
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
			float _PixelateVoxelX;
			float _PixelateVoxelY;
			float _PixelateVoxelZ;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.objVertex = v.vertex;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float2 uvSubImageBottomLeft = v.normal - float2(v.normal.x > 0 ? 1 : -1, v.normal.y > 0 ? 1 : -1);
				o.normal = v.normal - float3(uvSubImageBottomLeft, 0);
				float xScale = length(mul(_Object2World, float3(1, 0, 0))); 
				float zScale = length(mul(_Object2World, float3(0, 0, 1))); 
				o.extra = float4(abs(uvSubImageBottomLeft), zScale, xScale);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// NB: OpenGL has XY at lower left, which will be reflected in the vars
				float lightAmbient = 1.1;
				float lightRange = 0.3;
				float light = lightAmbient;

				fixed4 red = fixed4(1, 0, 0, 1);
				float2 textureSize = float2(_TextureWidth, _TextureHeight);
				float2 subImageSize = float2(_SubImageWidth, _SubImageHeight);

				float2 uvOnePixel = 1.0f / textureSize;
				float2 uvSubImageSize = subImageSize * uvOnePixel;

				float2 uvSubImageBottomLeft = float2(i.extra.x, i.extra.y);
				float2 uvAtlasClamped = clamp(i.uv, uvSubImageBottomLeft, uvSubImageBottomLeft + uvSubImageSize - (uvOnePixel / 2));

				float2 atlasPixel = uvAtlasClamped * textureSize;
				float2 atlasIndex = floor(atlasPixel / subImageSize);
				float2 uvInsideVoxel = frac(i.objVertex + 0.5);
				float2 subImagePixel = floor(atlasPixel % subImageSize) + uvInsideVoxel;
				float2 atlasPixelInt = floor(atlasPixel);
				float2 subImagePixelInt = floor(subImagePixel);

				float2 uvAtlasVoxelCenter = (atlasPixelInt + 0.5) * uvOnePixel;

				bool frontSide = (i.normal.z == -1);
				bool backSide = (i.normal.z == 1);
				bool bottomSide = (i.normal.y == -1);
				bool topSide = (i.normal.y == 1);
				bool leftSide = (i.normal.x == -1);
				bool rightSide = (i.normal.x == 1);

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

				float voxelDepthZ = i.extra.z / i.extra.w;
//				if (floor(voxelDepthZ) == 6) return red;

				float voxelPosZ = (i.objVertex.z + 0.5) * voxelDepthZ;

//				if (i.objVertex.z < 0) return red;
//				if (floor(voxelPosZ) == 2) return red;

				float3 lightPos;
				lightPos.x = (_PixelateVoxelX == 1 ? subImagePixelInt.x : subImagePixel.x) / subImageSize.x;
				lightPos.y = (_PixelateVoxelY == 1 ? subImagePixelInt.y : subImagePixel.y) / subImageSize.y;
				lightPos.z = (_PixelateVoxelZ == 1 ? floor(voxelPosZ) : voxelPosZ) / voxelDepthZ;
//				lightPos.z = voxelCountZ / i.objVertex.z;

				float3 lightDelta = lightPos * lightRange;

				if (frontSide) {
					light *= 1 + lightDelta.x + lightDelta.y;
				} else if (backSide) {
					light *= 0.5 + lightDelta.x / 3 + lightDelta.y / 3;
				} else if (bottomSide){
					light *= 0.5 + lightDelta.x / 3 - lightDelta.z / 3;
				} else if (topSide){
					light *= 0.9 + lightDelta.x - lightDelta.z;
				} else if (leftSide){
					light *= 0.5 + lightDelta.y / 3 - lightDelta.z / 3;
				} else if (rightSide){
					light *= 0.9 + lightDelta.y - lightDelta.z;
				}

				c *= light;

				return c;
			}
			ENDCG
		}
	}
}
