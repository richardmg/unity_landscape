﻿Shader "Custom/VoxelFrontBorderS"
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
			Cull Off

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
				float2 uvAtlas : POSITION2;
				float4 uvAtlasCubeRect : COLOR1;
				float4 extra : COLOR2;
			};

			// We only set correct normals for the side exclusive vertices
			// to be able to determine correct normals after interpolation
			// in the fragment shader.
			static float3 normalForCode[14] = {
				float3(-1, 0, 0),	// left exclusive
				float3(1, 0, 0),	// right exclusive
				float3(0, 0, 0),	// bottom exclusive (not used)
				float3(0, 0, 0),	// top exclusive (not used)
				float3(0, 0, -1),	// front exclusive
				float3(0, 0, 1),	// back exclusive
				float3(0, 0, 0),	// bottom left front
				float3(0, 0, 0),	// top left front
				float3(0, 0, 0),	// bottom right front
				float3(0, 0, 0),	// top right front
				float3(0, 0, 0),	// bottom left back
				float3(0, 0, 0),	// top left back
				float3(0, 0, 0),	// bottom right back
				float3(0, 0, 0),	// top right back
 			};

			static float3 vertexForCode[14] = {
				float3(0, 1, 1),	// left exclusive
				float3(1, 1, 0),	// right exclusive
				float3(0, 0, 1),	// bottom exclusive
				float3(0, 1, 1),	// top exclusive
				float3(0, 1, 0),	// front exclusive
				float3(1, 1, 1),	// back exclusive
				float3(0, 0, 0),	// bottom left front
				float3(0, 1, 0),	// top left front
				float3(1, 0, 0),	// bottom right front
				float3(1, 1, 0),	// top right front
				float3(0, 0, 1),	// bottom left back
				float3(0, 1, 1),	// top left back
				float3(1, 0, 1),	// bottom right back
				float3(1, 1, 1),	// top right back
 			};

			v2f vert (appdata v)
			{
				int vertexCode = (int)v.cubeDesc.b;
				float voxelDepth = v.cubeDesc.a / 100;

				float2 uvTextureSize = float2(_TextureWidth, _TextureHeight);
				float2 uvCubeBottomLeft = floor(v.uvAtlasCubeRectEncoded) / uvTextureSize;
				float2 uvCubeTopRight = frac(v.uvAtlasCubeRectEncoded) + (0.5 / uvTextureSize);

				float uvSubImageEffectiveWidth = frac(v.cubeDesc.a) * 2;
				float uvSubImageEffectiveHeight = frac(v.cubeDesc.b) * 2;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = mul(_Object2World, float4(v.normal, 0)).xyz;
				o.uvAtlas = v.cubeDesc.xy;
				o.uvAtlasCubeRect = float4(uvCubeBottomLeft, uvCubeTopRight);
				o.extra = float4(uvSubImageEffectiveWidth, uvSubImageEffectiveHeight, voxelDepth, 0);
				return o;
			}

			inline float2 uvSubImageClamp(float2 uv, v2f i)
			{
				float4 clampRect = i.uvAtlasCubeRect + _ClampOffset;
				return clamp(uv, clampRect.xy, clampRect.zw);
			}

			inline bool uvInsideSubImage(float2 uv, v2f i)
			{
				float4 clampRect = i.uvAtlasCubeRect + _ClampOffset;
				return (uv.x >= clampRect.x && uv.x <= clampRect.z && uv.y >= clampRect.y && uv.y <= clampRect.w);
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
//			return tex2Dlod(_MainTex, float4(uvSubImageClamp(i.uvAtlas, i), 0, 0));
				float2 textureSize = float2(_TextureWidth, _TextureHeight);
				float2 subImageSize = float2(_SubImageWidth, _SubImageHeight);
				float2 uvAtlasClamped = uvSubImageClamp(i.uvAtlas, i);
				float2 uvAtlasSubImageSize = subImageSize / textureSize;
				float2 subImageIndex = floor(uvAtlasClamped / uvAtlasSubImageSize);
				float2 uvSubImageBottomLeft = subImageIndex * uvAtlasSubImageSize;

				float2 uvSubImage = (uvAtlasClamped - uvSubImageBottomLeft) / uvAtlasSubImageSize;
				float2 uvEffectiveSubImage = uvSubImage / float3(i.extra.x, i.extra.y, 1);
				float2 voxelUnclamped = uvSubImage * subImageSize;
				float2 voxel = min(voxelUnclamped, subImageSize - 1);
				float2 uvVoxel = frac(voxelUnclamped);

				fixed4 c = uvInsideSubImage(i.uvAtlas, i) ? tex2Dlod(_MainTex, float4(i.uvAtlas, 0, 0)) : 0;

				if (c.a == 0) {
					// Draw top shadow
					float2 uvOneLine = 1 / textureSize;
					float2 uvShadowVoxel = i.uvAtlas - float2(uvOneLine.x * uvVoxel.y, uvOneLine.y);
					if (!uvInsideSubImage(uvShadowVoxel, i))
						uvShadowVoxel = 0;
					c = tex2Dlod(_MainTex, float4(uvShadowVoxel, 0, 0));

					if (c.a == 0) {
						// Draw right shadow
						float2 uvShadowVoxel = i.uvAtlas - uvOneLine;
						if (!uvInsideSubImage(uvShadowVoxel, i))
							uvShadowVoxel = 0;
						c = tex2Dlod(_MainTex, float4(uvShadowVoxel, 0, 0));
					}

					if (c.a == 0) {
						// Draw bottom shadow
						float2 uvShadowVoxel = i.uvAtlas - float2(uvOneLine.x, uvOneLine.y * uvVoxel.x);
						if (!uvInsideSubImage(uvShadowVoxel, i))
							uvShadowVoxel = 0;
						c = tex2Dlod(_MainTex, float4(uvShadowVoxel, 0, 0));
					}

					if (c.a == 0) {
						discard;
						return red;
					} else {
						// Return shadow color
						return c - _EdgeSharp;
						return fixed4(0, 0, 1, 1);
					}
				}

				////////////////////////////////////////////////////////
				// Apply lightning

				float sunDist = dot(i.normal, _SunPos);
				float sunAffection = pow(max(0, asin(sunDist)), _Attenuation);
				float sunLight = _Sunshine * sunAffection * _BaseLight;
				c *= max(_AmbientLight * _BaseLight, min(sunLight, _Sunshine * _Specular * _BaseLight));

				return c;
			}
			ENDCG
		}
	}
}