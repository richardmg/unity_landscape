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
// Upgrade NOTE: excluded shader from DX11 and Xbox360 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11 xbox360

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uvSubImageBottomLeft : TEXCOORD0;
				float2 unbatchedGeometry : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 objVertex : POSITION1;
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

			static float3 normalForCode[8] = {
				float3(-1, -1, -1),
				float3(-1, 1, -1),
				float3(1, -1, -1),
				float3(1, 1, -1),
				float3(-1, -1, 1),
				float3(-1, 1, 1),
				float3(1, -1, 1),
				float3(1, 1, 1)
 			};

			v2f vert (appdata v)
			{
				int normalCode = int(v.unbatchedGeometry.x * 10) - int(floor(v.unbatchedGeometry.x) * 10) - 1;
				int voxelDepth = int(v.unbatchedGeometry.y * 100) - int(floor(v.unbatchedGeometry.y) * 100);

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = normalForCode[normalCode];
				o.objVertex = float3(floor(v.unbatchedGeometry), o.normal.z == -1 ? 0 : voxelDepth);
				o.extra = float4(v.uvSubImageBottomLeft, voxelDepth, 0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 red = fixed4(1, 0, 0, 1);
				float2 textureSize = float2(_TextureWidth, _TextureHeight);
				float2 subImageSize = float2(_SubImageWidth, _SubImageHeight);

				float2 uvOnePixel = 1.0f / textureSize;
				float2 uvSubImageSize = subImageSize * uvOnePixel;
				float2 uvSubImageBottomLeft = float2(i.extra.x, i.extra.y);
				float2 uvAtlas = uvSubImageBottomLeft + clamp((i.objVertex.xy * uvOnePixel), 0, uvSubImageSize - (uvOnePixel / 2));

				float2 atlasPixel = uvAtlas * textureSize;
				float2 atlasIndex = floor(atlasPixel / subImageSize);
				float2 uvInsideVoxel = frac(i.objVertex);
				float2 subImagePixel = floor(atlasPixel % subImageSize) + uvInsideVoxel;
				float2 atlasPixelInt = floor(atlasPixel);
				float2 subImagePixelInt = floor(subImagePixel);

				float voxelDepth = i.extra.z;
				float voxelPosZ = i.objVertex.z;
				float uvInsideVoxelZ = frac(voxelPosZ);
				float2 uvAtlasVoxelCenter = atlasPixelInt * uvOnePixel;

				bool frontSide = (i.normal.z == -1);
				bool backSide = (i.normal.z == 1);
				bool leftSide = (i.normal.x == -1);
				bool rightSide = (i.normal.x == 1);
				// Since cubes (containing one or more voxels) on top of each other share vertices along the y-axis, the normals
				// will be wrong for the top-most cube. So we need to be a bit clever when calculating those sides.
				bool topSide = (i.normal.y == 1) && !(leftSide || rightSide || frontSide ||backSide);
				bool bottomSide = !(topSide || leftSide || rightSide || frontSide || backSide);

				////////////////////////////////////////////////////////
				// Get current voxel color

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

				////////////////////////////////////////////////////////
				// Calculate lights

				float3 lightPos;
				float lightRange = 0.3;
				float light = 1.1; // (ambient base)

				lightPos.x = (_PixelateVoxelX == 1 ? subImagePixelInt.x : subImagePixel.x) / subImageSize.x;
				lightPos.y = (_PixelateVoxelY == 1 ? subImagePixelInt.y : subImagePixel.y) / subImageSize.y;
				lightPos.z = (_PixelateVoxelZ == 1 ? floor(voxelPosZ) : voxelPosZ) / voxelDepth;

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

				////////////////////////////////////////////////////////

				return c;
			}
			ENDCG
		}
	}
}
