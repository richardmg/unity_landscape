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
		_AmbientLight ("Light ambient", Range(0, 2)) = 0.6
		_DirectionalLight ("Light directional", Range(0, 3)) = 1.5
		_Specular ("Light specular", Range(0, 3)) = 0.8
	}
	SubShader
	{
		Tags {
			"RenderType"="Opaque"
//          	"DisableBatching" = "True"
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
			#define M_PI 3.1415926535897932384626433832795

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
			float _Specular;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			static float _ClampOffset = 0.0001;
			static fixed4 red = fixed4(1, 0, 0, 1);
			static float3 _Sun = normalize(float3(1, 1, 0.5));

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uvAtlasCubeRectEncoded : TEXCOORD0;
				float4 cubeDesc : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 objNormal : NORMAL1;
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

 			inline float radBetween(float3 v1, float3 v2)
 			{
     			return acos(dot(v1, v2) / (length(v1) * length(v2)));
 			}

			v2f vert (appdata v)
			{
				float2 uvTextureSize = float2(_TextureWidth, _TextureHeight);
				float2 uvCubeBottomLeft = floor(v.uvAtlasCubeRectEncoded) / uvTextureSize;
				float2 uvCubeTopRight = frac(v.uvAtlasCubeRectEncoded) + (0.5 / uvTextureSize);
				float3 objNormal = normalForCode[(int)v.cubeDesc.b];

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = v.normal;
				o.objNormal = objNormal;
				o.uvAtlas = float3(v.cubeDesc.xy, (objNormal.z + 1) / 2);
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

 				int frontSide = int((i.objNormal.z - 1) / -2);
				int backSide = int((i.objNormal.z + 1) / 2);
				int leftSide = int(!frontSide) * int(!backSide) * int((i.objNormal.x - 1) / -2);
				int rightSide = int(!frontSide) * int(!backSide) * int((i.objNormal.x + 1) / 2);
				int topSide = int(!leftSide) * int(!rightSide) * int(!frontSide) * int(!backSide) * int((i.objNormal.y + 1) / 2);
				int bottomSide = int(!topSide) * int(!leftSide) * int(!rightSide) * int(!frontSide) * int(!backSide);

				float3 correctedNormal = i.normal * float3(1, (bottomSide | topSide), 1);
				correctedNormal = normalize(mul(_Object2World, correctedNormal));

				float rad = radBetween(correctedNormal, _Sun);
				float sunLight = min(_DirectionalLight * (1 - (rad / M_PI)), _DirectionalLight * _Specular);
				c *= max(_AmbientLight, sunLight);

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
