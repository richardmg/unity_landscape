﻿Shader "Custom/VoxelQuadS"
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
		_VoxelBorder("Voxel border", Range(0, 0.1)) = 0.1
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

			float _VoxelBorder;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			static float4 _ClampOffset = float4(0.0001, 0.0001, -0.0001, -0.0001);
			static fixed4 red = fixed4(1, 0, 0, 1);
			static float3 _SunPos = normalize(float3(0, 0, 1));

			static int kFaceDirectionX = 0;
			static int kFaceDirectionY = 1;
			static int kFaceDirectionZ = 2;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uvAtlasSubImageRectEncoded : TEXCOORD0;
				float4 cubeDesc : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 uvAtlas : POSITION2;
				float4 uvAtlasSubImageRect : COLOR1;
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

 			inline float ifSet(float testValue, float expr)
 			{
 				// testValue in [0, 1]
 				// Returns 1 if testValue == 0, otherwise expr
 				return 1 + (testValue * (expr - 1));
 			}

			v2f vert (appdata v)
			{
				float2 textureSize = float2(_TextureWidth, _TextureHeight);
				float2 uvSubImageBottomLeft = floor(v.uvAtlasSubImageRectEncoded) / textureSize;
				float2 uvSubImageTopRight = frac(v.uvAtlasSubImageRectEncoded) + (0.5 / textureSize);
				float normalCode = int(v.cubeDesc.b);

				float uvSubImageEffectiveWidth = frac(v.cubeDesc.a) * 2;
				float uvSubImageEffectiveHeight = frac(v.cubeDesc.b) * 2;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = mul(_Object2World, float4(v.normal, 0)).xyz;
				o.uvAtlas = float3(v.cubeDesc.xy, 0);
				o.uvAtlasSubImageRect = float4(uvSubImageBottomLeft, uvSubImageTopRight);

				float2 subImageSize = float2(_SubImageWidth, _SubImageHeight);
				float2 uvAtlasSubImageSize = subImageSize / textureSize;
				float2 uvSubImage = (o.uvAtlas.xy - uvSubImageBottomLeft) / uvAtlasSubImageSize;
				float2 uvSubImageOneLine = 1 / subImageSize;
				float faceDirection = kFaceDirectionZ;

				if (uvSubImage.x >= uvSubImageOneLine.x && uvSubImage.x < 1)
					faceDirection = kFaceDirectionX;
				else if (uvSubImage.y >= uvSubImageOneLine.y && uvSubImage.y < 1)
					faceDirection = kFaceDirectionY;

				o.extra = float4(uvSubImageEffectiveWidth, uvSubImageEffectiveHeight, normalCode, faceDirection);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				////////////////////////////////////////////////////////
				// Start by calculating an API that we can use below

				// Move to static
				float3 textureSize = float3(_TextureWidth, _TextureHeight, i.extra.z);
				float3 uvAtlasOnePixel = 1.0f / textureSize;
				float3 uvAtlasHalfPixel = uvAtlasOnePixel / 2;

				float4 clampRect = i.uvAtlasSubImageRect + _ClampOffset;
				float3 uvAtlasClamped = clamp(i.uvAtlas, float3(clampRect.xy, 0), float3(clampRect.zw, (1 - _ClampOffset.x)));

				float3 subImageSize = float3(_SubImageWidth, _SubImageHeight, i.extra.z);
				float3 uvAtlasSubImageSize = subImageSize / textureSize;
				float3 subImageIndex = float3(floor(uvAtlasClamped / uvAtlasSubImageSize).xy, 0);
				float3 uvSubImageBottomLeft = subImageIndex * uvAtlasSubImageSize;

				float3 uvSubImage = (uvAtlasClamped - uvSubImageBottomLeft) / uvAtlasSubImageSize;
				float3 uvEffectiveSubImage = uvSubImage / float3(i.extra.x, i.extra.y, 1);
				float3 voxelUnclamped = uvSubImage * subImageSize;
				float3 voxel = min(voxelUnclamped, subImageSize - 1);
				float3 uvVoxel = frac(voxelUnclamped);

				int faceDirection = (int)i.extra.w;

				////////////////////////////////////////////////////////
				// Fetch main atlas color

				fixed4 c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy, 0, 0));

				if (c.a == 0) {
					// We use one vertical / horisontal quad to draw voxel faces on both sides of the quad.
					// So if there is no voxel on one side of the quad, we check the other side 
					if (faceDirection == kFaceDirectionX) {
						c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy - float2(uvAtlasOnePixel.x, 0), 0, 0));
						if (c.a != 0)
							voxel.x -= 1;
					} else if (faceDirection == kFaceDirectionY) {
						c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy - float2(0, uvAtlasOnePixel.y), 0, 0));
						if (c.a != 0)
							voxel.y -= 1;
					}

					if (c.a == 0) {
						// At integer coordinates, where the horisontal / vertical quads meet, we'll get cracks at
						// the edges whenever we end up switching which voxel face we draw above. The reason is that
						// perpendicualar planes to the quad that switched side, needs to do the same switching.
						// Since it's hard to detect exactly when we are at integer coordidates (according to
						// text2Dlod), we stretch the perpendicular side to be a bit wider.
						float border = _VoxelBorder; // move to static
						if (faceDirection == kFaceDirectionX) {
							if (uvVoxel.y < border || uvVoxel.y > 1 - border) {
								voxel.y -= 1;
								uvVoxel.y = 1;
								c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy - float2(0, uvAtlasHalfPixel.y), 0, 0));
								if (c.a != 0) {
									voxel.y -= 1;
									uvVoxel.y = 1;
								} else {
									c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy - uvAtlasHalfPixel, 0, 0));
									if (c.a != 0) {
										voxel.x -= 1;
										uvVoxel.x = 1;
									}
								}
							}
						} else if (faceDirection == kFaceDirectionY) {
							if (uvVoxel.x < border || uvVoxel.x > 1 - border) {
								c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy - float2(uvAtlasHalfPixel.x, 0), 0, 0));
								if (c.a != 0) {
									voxel.x -= 1;
									uvVoxel.x = 1;
								} else {
									c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy - uvAtlasHalfPixel, 0, 0));
									if (c.a != 0) {
										voxel.y -= 1;
										uvVoxel.y = 1;
									}
								}
							}
						} else {
							if (uvVoxel.x < border || uvVoxel.x > 1 - border) {
								c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy - float2(uvAtlasHalfPixel.x, 0), 0, 0));
								if (c.a != 0) {
									voxel.x -= 1;
									uvVoxel.x = 1;
								} else if (uvVoxel.y < border || uvVoxel.y > 1 - border) {
									c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy - uvAtlasHalfPixel, 0, 0));
									if (c.a != 0) {
										voxel.y -= 1;
										uvVoxel.y = 1;
									}
								}
							}
						}
					}
				}

				if (c.a == 0) {
					discard;
					return c;
				}

				////////////////////////////////////////////////////////
				// Apply lightning

				float sunDist = dot(i.normal, _SunPos);
				float sunAffection = pow(max(0, asin(sunDist)), _Attenuation);
				float sunLight = _Sunshine * sunAffection * _BaseLight;
				c *= max(_AmbientLight * _BaseLight, min(sunLight, _Sunshine * _Specular * _BaseLight));
					
				////////////////////////////////////////////////////////
				// Apply alternate voxel color

				int3 voxelate = int3(voxel * float3(_VoxelateX, _VoxelateY, _VoxelateZ));
				c *= 1 + ((voxelate.x + voxelate.y + voxelate.z) % 2) *  _VoxelateStrength;

				////////////////////////////////////////////////////////
				// Sharpen contrast at edges

				float sharpenEdge = 1 + (!(faceDirection & kFaceDirectionZ) * -_EdgeSharp * _BaseLight);
				c *= sharpenEdge;

				////////////////////////////////////////////////////////
				// Apply gradient

				float gradientStrength = (((sign(sunDist) + 1) / 2) * _GradientSunSide) + (((sign(sunDist) - 1) / -2) * _GradientShadeSide);
				gradientStrength = min(gradientStrength, abs(sunDist) * gradientStrength);
				float gradientSide = (1 - gradientStrength) + (uvEffectiveSubImage.y * gradientStrength);
				c *= 1 + ((faceDirection & kFaceDirectionZ) * (gradientSide - 1) * _BaseLight);

				////////////////////////////////////////////////////////

				c = clamp(c, 0, 1);

				return c;
			}

			ENDCG
		}
	}
}
