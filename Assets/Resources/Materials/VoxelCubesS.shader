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
				float2 uvAtlasCubeRectEncoded : TEXCOORD0;
				float4 cubeDesc : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 uvAtlas : POSITION2;
				float4 uvAtlasCubeRect : COLOR1;
				float4 extra : COLOR2;
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
				float3 normal = normalForCode[(int)v.cubeDesc.b];
				float voxelDepth = v.cubeDesc.a;
				float2 uvTextureSize = float2(_TextureWidth, _TextureHeight);
				float2 uvCubeBottomLeft = floor(v.uvAtlasCubeRectEncoded) / uvTextureSize;
				float2 uvCubeTopRight = frac(v.uvAtlasCubeRectEncoded) + (0.5 / uvTextureSize);
				float2 uvAtlas = v.cubeDesc.xy;
				float uvCubeZ = (normal.z + 1) / 2;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = normal;
				o.uvAtlas = float3(uvAtlas, uvCubeZ);
				o.uvAtlasCubeRect = float4(uvCubeBottomLeft, uvCubeTopRight);
				o.extra = float4(0, 0, voxelDepth, 0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Since we only use eight vertices per cube, the result will be that normals at the edges
				// (which reflect the uninterpolated state of the vertex), will report that the pixel belongs
				// to two different sides (even three in the corners). To avoid egde seams, we need to do some
				// extra checking to ensure that the sides end up exclusive. This still results in some minor
				// drawing artifacts when a cube is seen from e.g back-left, since then the front side will bleed
				// through on the edges. This can probably be fixed by including view direction into the mix.
 				int frontSide = int((i.normal.z - 1) / -2);
				int backSide = int((i.normal.z + 1) / 2);
				int leftSide = int(!frontSide) * int(!backSide) * int((i.normal.x - 1) / -2);
				int rightSide = int(!frontSide) * int(!backSide) * int((i.normal.x + 1) / 2);
				int topSide = int(!leftSide) * int(!rightSide) * int(!frontSide) * int(!backSide) * int((i.normal.y + 1) / 2);
				int bottomSide = int(!topSide) * int(!leftSide) * int(!rightSide) * int(!frontSide) * int(!backSide);

				float2 textureSize = float2(_TextureWidth, _TextureHeight);
				float2 uvAtlasOnePixel = 1.0f / textureSize;
				float4 clampRect = i.uvAtlasCubeRect - float4(0, 0, uvAtlasOnePixel / 2);
				float2 uvAtlasClamped = clamp(i.uvAtlas.xy, clampRect.xy, clampRect.zw);
				float3 uvVoxel = float3(frac((i.uvAtlas.xy - i.uvAtlasCubeRect.xy) * textureSize), frac(i.uvAtlas.z * i.extra.z));

				fixed4 c = tex2Dlod(_MainTex, float4(uvAtlasClamped, 0, 0));

				////////////////////////////////////////////////////////
				// Calculate lights

				float2 subImageSize = float2(_SubImageWidth, _SubImageHeight);
				float2 uvAtlasSubImageSize = subImageSize / textureSize;
				float2 uvSubImageOnePixel = 1 / subImageSize;
				float2 subImageIndex = floor(uvAtlasClamped / uvAtlasSubImageSize);
				float2 uvSubImageBottomLeft = subImageIndex * uvAtlasSubImageSize;
				float2 uvSubImage = (i.uvAtlas.xy - uvSubImageBottomLeft) / uvAtlasSubImageSize;
				float2 uvSubImageFlat = floor(uvSubImage / uvSubImageOnePixel) * uvSubImageOnePixel;
				float uvAtlasZFlat = floor(i.uvAtlas.z * i.extra.z) / i.extra.z;

				float3 lightPos;
				lightPos.x = ((_PixelateVoxelX * uvSubImageFlat.x) + (!_PixelateVoxelX * uvSubImage.x));
				lightPos.y = ((_PixelateVoxelY * uvSubImageFlat.y) + (!_PixelateVoxelY * uvSubImage.y));
				lightPos.z = ((_PixelateVoxelZ * uvAtlasZFlat) + (!_PixelateVoxelZ * i.uvAtlas.z)) ;

				float lightRange = 0.4;
				float3 lightDelta = lightPos * lightRange;

				float light = (backSide * (0.1 + lightDelta.x / 2 + lightDelta.y / 2))
						+ (bottomSide * (0.1 + lightDelta.x / 2 + lightDelta.y / 2))
						+ (leftSide * (0.1 + lightDelta.y / 2 - lightDelta.z / 2))
						+ (frontSide * (0.4 + lightDelta.x + lightDelta.y))
						+ (topSide * (0.4 + lightDelta.x - lightDelta.z))
						+ (rightSide * (0.4 + lightDelta.y - lightDelta.z))
						;

				c *= 0.7 + light;

				////////////////////////////////////////////////////////

				return c;
			}
			ENDCG
		}
	}
}
