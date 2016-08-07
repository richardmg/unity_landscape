Shader "Custom/VoxelQuadS"
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
		_Attenuation ("Attenuation", Range(0.0001, 1.0)) = 0.3
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
//			Cull off

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

			static float4 _ClampOffset = float4(0.0001, 0.0001, -0.0001, -0.0001);
			static fixed4 red = fixed4(1, 0, 0, 1);
			static float3 _SunPos = normalize(float3(0, 0, 1));

			static int kFaceUnknown = 0;
			static int kFaceLeft = 1;
			static int kFaceRight = 2;
			static int kFaceBottom = 4;
			static int kFaceTop = 8;
			static int kFaceFront = 16;
			static int kFaceBack = 32;
			static int kFaceMiddle = 64;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float4 cubeDesc : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 uvAtlas : POSITION2;
				float4 extra : COLOR2;
			};

 			inline int isOne(float value)
 			{
 			// FIXME, avoid using if test
 			return value == 1;
 				return int((value + 1) / 2);
 			}

 			inline int isNull(float value)
 			{
 			// FIXME, avoid using if test
 				return value == 0;
 				return abs(!value);
 			}

 			inline float ifTrue(float testValue, float expr)
 			{
 				// testValue in [0, 1]
 				// Returns 1 if testValue == 0, otherwise expr
 				return 1 + (sign(abs(testValue)) * (expr - 1));
 			}

			v2f vert (appdata v)
			{
				float2 textureSize = float2(_TextureWidth, _TextureHeight);
				float face = int(v.cubeDesc.b);
				float uvZ = frac(v.cubeDesc.b) * 2;
				float zDepth = v.cubeDesc.a;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = mul(_Object2World, float4(v.normal, 0)).xyz;
				o.uvAtlas = float3(v.uv, uvZ);
				o.extra = float4(0, 0, zDepth, face);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				////////////////////////////////////////////////////////
				// Start by calculating an API that we can use below

				// Move to static
				int face = round(i.extra.w);
				float3 textureSize = float3(_TextureWidth, _TextureHeight, i.extra.z);
				float3 uvAtlasOnePixel = 1.0f / textureSize;

				float3 subImageSize = float3(_SubImageWidth, _SubImageHeight, i.extra.z);
				float3 uvAtlasSubImageSize = subImageSize / textureSize;
				float3 subImageIndex = float3(floor(i.uvAtlas / uvAtlasSubImageSize).xy, 0);
				float3 uvSubImageBottomLeft = subImageIndex * uvAtlasSubImageSize;

				float3 uvSubImage = (i.uvAtlas - uvSubImageBottomLeft) / uvAtlasSubImageSize;
				float3 voxelUnclamped = uvSubImage * subImageSize;
				float3 voxel = min(voxelUnclamped, subImageSize - 1);
				float3 uvVoxel = frac(voxelUnclamped);

				////////////////////////////////////////////////////////
				// Fetch main atlas color

				fixed4 c = tex2Dlod(_MainTex, float4(i.uvAtlas.xy, 0, 0));

				if (c.a == 0) {
					discard;
					return c;
				}

				////////////////////////////////////////////////////////
				// Apply lightning

				float sunDist = dot(normalize(i.normal), _SunPos);
				float sunAffection = pow(max(0, asin(sunDist)), _Attenuation);
				float sunLight = _Sunshine * sunAffection * _BaseLight;
				c *= max(_AmbientLight * _BaseLight, min(sunLight, _Sunshine * _Specular * _BaseLight));
					
				////////////////////////////////////////////////////////
				// Apply alternate voxel color

				int3 voxelate = int3(voxel * float3(_VoxelateX, _VoxelateY, _VoxelateZ));
				c *= ifTrue((voxelate.x + voxelate.y + voxelate.z) % 2, 1 + _VoxelateStrength);

				////////////////////////////////////////////////////////
				// Sharpen contrast at edges

				c *= ifTrue(face & (kFaceFront | kFaceBack), 1 + (_EdgeSharp * _BaseLight));

				////////////////////////////////////////////////////////

				c = clamp(c, 0, 1);
				return c;
			}

			ENDCG
		}
	}
}
