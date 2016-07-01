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
		_AmbientLight ("Light ambient", Range(0, 2)) = 0.7
		_DirectionalLight ("Light directional", Range(0, 1)) = 0.4
		_LightAtt ("Light attenuation", Range(0, 1)) = 0.5
		_LightShade ("Light shade", Range(0, 1)) = 0.2
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

			int _TextureWidth;
			int _TextureHeight;
			int _SubImageWidth;
			int _SubImageHeight;

			float _PixelateVoxelX;
			float _PixelateVoxelY;
			float _PixelateVoxelZ;
			float _AmbientLight;
			float _DirectionalLight;
			float _LightAtt;
			float _LightShade;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			static float _ClampOffset = 0.0001;
			static fixed4 red = fixed4(1, 0, 0, 1);

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
				float2 uvTextureSize = float2(_TextureWidth, _TextureHeight);
				float2 uvCubeBottomLeft = floor(v.uvAtlasCubeRectEncoded) / uvTextureSize;
				float2 uvCubeTopRight = frac(v.uvAtlasCubeRectEncoded) + (0.5 / uvTextureSize);

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = normalForCode[(int)v.cubeDesc.b];
				o.uvAtlas = float3(v.cubeDesc.xy, (o.normal.z + 1) / 2);
				o.uvAtlasCubeRect = float4(uvCubeBottomLeft, uvCubeTopRight);
				o.extra = float4(0, 0, v.cubeDesc.a, 0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				////////////////////////////////////////////////////////
				// Fetch main atlas color

				float2 textureSize = float2(_TextureWidth, _TextureHeight);
				float2 uvAtlasOnePixel = 1.0f / textureSize;
				float4 clampRect = i.uvAtlasCubeRect - float4(0, 0, _ClampOffset, _ClampOffset);
				float3 uvAtlasClamped = clamp(i.uvAtlas, float3(clampRect.xy, 0), float3(clampRect.zw, (1 - _ClampOffset)));
				fixed4 c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy, 0, 0));

				////////////////////////////////////////////////////////
				// Fetch detail image
				float3 uvVoxel = float3(frac((uvAtlasClamped.xy - i.uvAtlasCubeRect.xy) * textureSize), frac(uvAtlasClamped.z * i.extra.z));

				////////////////////////////////////////////////////////
				// Apply lightning

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

				float2 subImageSize = float2(_SubImageWidth, _SubImageHeight);
				float2 uvAtlasSubImageSize = subImageSize / textureSize;
				float2 uvSubImageOnePixel = 1 / subImageSize;
				float2 subImageIndex = floor(uvAtlasClamped / uvAtlasSubImageSize);
				float2 uvSubImageBottomLeft = subImageIndex * uvAtlasSubImageSize;
				float2 uvSubImage = (uvAtlasClamped - uvSubImageBottomLeft) / uvAtlasSubImageSize;
				float2 uvSubImageFlat = floor(uvSubImage / uvSubImageOnePixel) * uvSubImageOnePixel;
				float uvAtlasZFlat = floor(i.uvAtlas.z * i.extra.z) / i.extra.z;

				float3 lightPos;
				lightPos.x = ((_PixelateVoxelX * uvSubImageFlat.x) + (!_PixelateVoxelX * uvSubImage.x));
				lightPos.y = ((_PixelateVoxelY * uvSubImageFlat.y) + (!_PixelateVoxelY * uvSubImage.y));
				lightPos.z = ((_PixelateVoxelZ * uvAtlasZFlat) + (!_PixelateVoxelZ * i.uvAtlas.z));

				float3 sunSideGradient = _DirectionalLight * (_LightAtt + (lightPos * (1 - _LightAtt)));
				float3 shadeSideGradient = sunSideGradient * _LightShade;

				float directionalLight =
						+ (bottomSide	* (shadeSideGradient.x + shadeSideGradient.y))
						+ (leftSide		* (shadeSideGradient.y + shadeSideGradient.z))
						+ (frontSide	* (shadeSideGradient.x + shadeSideGradient.y))
						+ (backSide		* (sunSideGradient.x + sunSideGradient.y))
						+ (topSide		* (sunSideGradient.x + sunSideGradient.z))
						+ (rightSide	* (sunSideGradient.y + sunSideGradient.z));

				c *= _AmbientLight + directionalLight;

				////////////////////////////////////////////////////////
				// Apply alternate voxel color

//				float3 voxelPosSubImage = float3(uvSubImage * subImageSize, uvZClamped * i.extra.z);
//				float3 voxelPosSubImageClamped = clamp(voxelPosSubImage, 0.0, float3(subImageSize - 1.0, i.extra.z - 1.0));
//				int3 alt = int3(voxelPosSubImage % 2);

//				if (alternateX) return red;
//				if (alt.x && alt.y) return red;
//				if (!alt.x && !alt.y) return red;
//				if (alt.y) return red;
//				if (alternateZ) return red;
//				c += voxelPosSubImage.x % 2 * 0.2;
//				c += (voxelPosSubImage.z - 1) % 2 * 0.2;

				////////////////////////////////////////////////////////

				return c;
			}
			ENDCG
		}
	}
}
