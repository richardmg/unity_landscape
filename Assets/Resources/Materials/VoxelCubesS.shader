Shader "Custom/VoxelCubesS"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TextureWidth ("Texture width", Int) = 64
		_TextureHeight ("Texture height", Int) = 64
		_SubImageWidth ("Subimage width", Int) = 16
		_SubImageHeight ("Subimage height", Int) = 8
		_PixelateStrength ("Pixelate strength", Range(0, 0.1)) = 0.05
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
			float _PixelateStrength;

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

				float3 textureSize = float3(_TextureWidth, _TextureHeight, i.extra.z);
				float3 uvAtlasOnePixel = 1.0f / textureSize;
				float4 clampRect = i.uvAtlasCubeRect - float4(0, 0, _ClampOffset, _ClampOffset);
				float3 uvAtlasClamped = clamp(i.uvAtlas, float3(clampRect.xy, 0), float3(clampRect.zw, (1 - _ClampOffset)));
				fixed4 c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy, 0, 0));

				float3 subImageSize = float3(_SubImageWidth, _SubImageHeight, i.extra.z);
				float3 uvAtlasSubImageSize = subImageSize / textureSize;
				float3 subImageIndex = float3(floor(uvAtlasClamped / uvAtlasSubImageSize).xy, 0);
				float3 uvSubImageBottomLeft = subImageIndex * uvAtlasSubImageSize;

				float3 uvSubImage = (uvAtlasClamped - uvSubImageBottomLeft) / uvAtlasSubImageSize;
				float3 voxel = uvSubImage * subImageSize;
				float3 uvVoxel = frac(voxel);

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

				float3 sunSideGradient = _DirectionalLight * (_LightAtt + (uvSubImage * (1 - _LightAtt)));
				float3 shadeSideGradient = sunSideGradient * _LightShade;

				float directionalLight =
						+ (bottomSide	* (0.5 +  + shadeSideGradient.y))
						+ (leftSide		* (shadeSideGradient.y + shadeSideGradient.z))
						+ (frontSide	* (0.5 +  + shadeSideGradient.y))
						+ (backSide		* (0.5 + sunSideGradient.y))
						+ (topSide		* (0.5 +  + sunSideGradient.z))
						+ (rightSide	* (sunSideGradient.y + sunSideGradient.z));

				c *= _AmbientLight + directionalLight;

				////////////////////////////////////////////////////////
				// Apply alternate voxel color

				int3 voxelate = int3(voxel * float3(_PixelateVoxelX, _PixelateVoxelY, _PixelateVoxelZ));
				c *= 1 + (((voxelate.x + voxelate.y + voxelate.z) % 2) * _PixelateStrength);

				////////////////////////////////////////////////////////

				return c;
			}
			ENDCG
		}
	}
}
