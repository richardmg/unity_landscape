Shader "Custom/VoxelCubesS"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TextureWidth ("Texture width", Int) = 64
		_TextureHeight ("Texture height", Int) = 64
		_SubImageWidth ("Subimage width", Int) = 16
		_SubImageHeight ("Subimage height", Int) = 8
		_GradientSunSide ("Gradient sunside", Range(0, 1)) = 0.1
		_GradientShadeSide ("Gradient shadeside", Range(0, 1)) = 0.5
		_VoxelateStrength ("Voxelate strength", Range(0, 0.1)) = 0.05
		_VoxelateX ("Voxelate X", Range(0, 1)) = 1
		_VoxelateY ("Voxelate Y", Range(0, 1)) = 1
		_VoxelateZ ("Voxelate Z", Range(0, 1)) = 1
		_BaseLight ("Base light", Range(0, 2)) = 0.85
		_AmbientLight ("Ambient", Range(0, 2)) = 1.1
		_Sunshine ("Sunshine", Range(0, 3)) = 1.6
		_Specular ("Specular", Range(0, 1)) = 0.8
		_Attenuation ("Attenuation", Range(0.0001, 0.5)) = 0.3
		_EdgeSharp ("Sharpen edge", Range(0, 0.3)) = 0.2
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

			float _VoxelateX;
			float _VoxelateY;
			float _VoxelateZ;
			float _VoxelateStrength;

			float _GradientSunSide;
			float _GradientShadeSide;

			float _BaseLight;
			float _AmbientLight;
			float _Sunshine;
			float _Specular;
			float _EdgeSharp;
			float _Attenuation;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			static float _ClampOffset = 0.0001;
			static fixed4 red = fixed4(1, 0, 0, 1);
			static float3 _SunPos = normalize(float3(0, 1, 1));

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

			v2f vert (appdata v)
			{
				float2 uvTextureSize = float2(_TextureWidth, _TextureHeight);
				float2 uvCubeBottomLeft = floor(v.uvAtlasCubeRectEncoded) / uvTextureSize;
				float2 uvCubeTopRight = frac(v.uvAtlasCubeRectEncoded) + (0.5 / uvTextureSize);
				float uvEffectiveHeight = frac(v.cubeDesc.b) * 2;
				float3 objNormal = normalForCode[(int)v.cubeDesc.b];
				float voxelDepth = v.cubeDesc.a;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = mul(_Object2World, v.normal);
				o.objNormal = objNormal;
				o.uvAtlas = float3(v.cubeDesc.xy, (objNormal.z + 1) / 2);
				o.uvAtlasCubeRect = float4(uvCubeBottomLeft, uvCubeTopRight);
				o.extra = float4(0, uvEffectiveHeight, voxelDepth, 0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				////////////////////////////////////////////////////////
				// Start by calculating an API that we can use below

				float3 textureSize = float3(_TextureWidth, _TextureHeight, i.extra.z);
				float3 uvAtlasOnePixel = 1.0f / textureSize;
				float4 clampRect = i.uvAtlasCubeRect - float4(0, 0, _ClampOffset, _ClampOffset);
				float3 uvAtlasClamped = clamp(i.uvAtlas, float3(clampRect.xy, 0), float3(clampRect.zw, (1 - _ClampOffset)));

				float3 subImageSize = float3(_SubImageWidth, _SubImageHeight, i.extra.z);
				float3 uvAtlasSubImageSize = subImageSize / textureSize;
				float3 subImageIndex = float3(floor(uvAtlasClamped / uvAtlasSubImageSize).xy, 0);
				float3 uvSubImageBottomLeft = subImageIndex * uvAtlasSubImageSize;

				float3 uvSubImage = (uvAtlasClamped - uvSubImageBottomLeft) / uvAtlasSubImageSize;
				float3 uvEffectiveSubImage = uvSubImage / float3(1, i.extra.y, 1);
				float3 voxel = uvSubImage * subImageSize;
				float3 uvVoxel = frac(voxel);

 				int frontSide = int((i.objNormal.z - 1) / -2);
				int backSide = int((i.objNormal.z + 1) / 2);
				int leftSide = int(!frontSide) * int(!backSide) * int((i.objNormal.x - 1) / -2);
				int rightSide = int(!frontSide) * int(!backSide) * int((i.objNormal.x + 1) / 2);
				int topSide = int(!leftSide) * int(!rightSide) * int(!frontSide) * int(!backSide) * int((i.objNormal.y + 1) / 2);
				int bottomSide = int(!topSide) * int(!leftSide) * int(!rightSide) * int(!frontSide) * int(!backSide);

				////////////////////////////////////////////////////////
				// Fetch main atlas color
				fixed4 c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy, 0, 0));

				////////////////////////////////////////////////////////
				// Apply lightning

				float sunDist = dot(i.normal, _SunPos);
				float sunAffection = pow(max(0, asin(sunDist)), _Attenuation);
				float sunLight = _Sunshine * sunAffection * _BaseLight;
				c *= max(_AmbientLight * _BaseLight, min(sunLight, _Sunshine * _Specular * _BaseLight));

				////////////////////////////////////////////////////////
				// Apply alternate voxel color

				int3 voxelate = int3(voxel * float3(_VoxelateX, _VoxelateY, _VoxelateZ));
				c *= 1 + (((voxelate.x + voxelate.y + voxelate.z) % 2) * _VoxelateStrength);

				////////////////////////////////////////////////////////
				// Sharpen contrast at cube edges

				float sharpLeft = 1 + ((leftSide | rightSide) * -_EdgeSharp * _BaseLight);
				float sharpTop = 1 + ((topSide | bottomSide) * _EdgeSharp * _BaseLight);
				c *= sharpLeft * sharpTop;

				////////////////////////////////////////////////////////
				// Apply gradient

				float gradientStrength = (((sign(sunDist) + 1) / 2) * _GradientSunSide) + (((sign(sunDist) - 1) / -2) * _GradientShadeSide);
				gradientStrength = min(gradientStrength, abs(sunDist) * gradientStrength);
				float gradientSide = (1 - gradientStrength) + (uvEffectiveSubImage.y * gradientStrength * sharpLeft);
				c *= 1 + ((frontSide | backSide | leftSide | rightSide) * (gradientSide - 1) * _BaseLight);
				float gradientTop = (1 - gradientStrength) + (uvEffectiveSubImage.x * gradientStrength * sharpTop);
				c *= 1 + ((topSide | bottomSide) * (gradientTop - 1) * _BaseLight);

				////////////////////////////////////////////////////////

				return c;
			}
			ENDCG
		}
	}
}
