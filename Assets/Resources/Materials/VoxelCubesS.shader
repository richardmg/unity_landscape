﻿Shader "Custom/VoxelCubesS"
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
				float4 unbatchedGeometry : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 objVertex : COLOR;
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

			static fixed4 red = fixed4(1, 0, 0, 1);

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
				int normalCode = (int)v.unbatchedGeometry.b;
				float voxelDepth = v.unbatchedGeometry.a;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = normalForCode[normalCode];
				o.objVertex = float3(v.unbatchedGeometry.rg, (o.normal.z - 1) * voxelDepth / -2);
				o.extra = float4(v.uvSubImageBottomLeft, voxelDepth, 0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Since adjacent cubes share vertices, the normals will sometimes end up wrong for one of the
				// cubes along the y-axis. So we need to check for topSide and bottomSide a bit differently
				// (which is also why we cannot calculate this directly in the vertex shader).
				bool frontSide = int((i.normal.z - 1) / -2);
				bool backSide = int((i.normal.z + 1) / 2);
				bool leftSide = int((i.normal.x - 1) / -2);
				bool rightSide = int((i.normal.x + 1) / 2);
				bool topSide = int((i.normal.y + 1) / 2) * int(!leftSide) * int(!rightSide) * int(!frontSide) * int(!backSide);
				bool bottomSide = int(!topSide) * int(!leftSide) * int(!rightSide) * int(!frontSide) * int(!backSide);

				float2 textureSize = float2(_TextureWidth, _TextureHeight);
				float2 subImageSize = float2(_SubImageWidth, _SubImageHeight);

				float2 uvOnePixel = 1.0f / textureSize;
				float2 uvHalfPixel = uvOnePixel / 2;
				float2 uvSubImageSize = subImageSize * uvOnePixel;
				float2 uvSubImageBottomLeft = float2(i.extra.x, i.extra.y);
				float2 uvInsideVoxel = frac(i.objVertex);
				float2 uvInsideSubImageClamped = clamp((i.objVertex.xy * uvOnePixel), 0, uvSubImageSize - uvHalfPixel);
				float2 uvTopAndRightSideAdjustment = uvHalfPixel * float2(int(rightSide), int(topSide));
				float2 uvAtlas = uvSubImageBottomLeft + uvInsideSubImageClamped - uvTopAndRightSideAdjustment;

				float2 atlasPixel = uvAtlas * textureSize;
				float2 atlasIndex = floor(atlasPixel / subImageSize);
				float2 subImagePixel = floor(atlasPixel % subImageSize) + uvInsideVoxel;
				float2 atlasPixelInt = floor(atlasPixel);
				float2 subImagePixelInt = floor(subImagePixel);

				float voxelDepth = i.extra.z;
				float voxelPosZ = i.objVertex.z;
				float uvInsideVoxelZ = frac(voxelPosZ);
				float2 uvAtlasVoxelCenter = atlasPixelInt * uvOnePixel;

				////////////////////////////////////////////////////////
				// Get current voxel color

				fixed4 c = tex2Dlod(_MainTex, float4(uvAtlasVoxelCenter, 0, 0));

				if (c.a == 0) {
					// Always sample opaque pixels. Transparent pixels are not converted to voxels, but
					// will still bleed in from the edges when drawing opaque voxels unless we do this clamping.
					float seam = 0.5f;
					float oneMinusSeam = 1 - seam;
					bool leftEdge = uvInsideVoxel.x < seam;
					bool rightEdge = uvInsideVoxel.x > oneMinusSeam;
					bool topEdge = uvInsideVoxel.y > oneMinusSeam;
					bool bottomEdge = uvInsideVoxel.y < seam;

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
