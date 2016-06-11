﻿Shader "Custom/VoxelVolumeUnlitS"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TextureWidth ("Texture width", Int) = 64
		_TextureHeight ("Texture height", Int) = 64
		_SubImageWidth ("Subimage width", Int) = 16
		_SubImageHeight ("Subimage height", Int) = 8
	}
	SubShader
	{
		Tags {
			"RenderType"="Opaque"
			"Queue" = "AlphaTest"
		}

		LOD 100

		Pass
		{
      	 	Cull Off
      	 	ZTest Less

			CGPROGRAM

			#define USE_LIGHT
			#define DEBUG_TEXTURE_ATLAS

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 objectVertex : POSITION2;
				float3 normal : NORMAL;
			 	// x: left or right edge, y: top or bottom edge, z: z scale, w: isBackFace
				float4 extra : COLOR1;
			};

			int _TextureWidth;
			int _TextureHeight;
			int _SubImageWidth;
			int _SubImageHeight;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.objectVertex = v.vertex;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;

				float3 worldPos = mul(_Object2World, v.vertex).xyz;
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float isBackFace = dot(worldNormal, worldViewDir) > 0 ? 0 : 1; 

				if (isBackFace) {
					float vx = v.vertex.x;
					float vy = v.vertex.y;
                	float nx = v.normal.x;
                	float ny = v.normal.y;

					if (nx == 0 && ny == 0) {
						// We have a front or back face. Create
						// degenerate triangle to cull it away
						o.vertex = 0;
					} else if (nx != 0) {
						// Left or righ edge
					 	if (vx == 0 || vx == _SubImageWidth)
							o.vertex = 0;
					} else if (ny != 0) {
						// Left or right edge
					 	if (vy == 0 || vy == _SubImageHeight)
							o.vertex = 0;
					}
				}

				float zScale = length(mul(_Object2World, float3(0, 0, 1)));

				o.extra.x = v.vertex.x;
				o.extra.y = v.vertex.y;
				o.extra.z = zScale;
				o.extra.w = isBackFace;

//				o.lightDir = ObjSpaceLightDir( v.vertex );

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float lightMax = 0.5;
				float lightDampning = 0.02;
				float light = 1;

				float uvOnePixelX = (1.0 / _TextureWidth);
				float uvOnePixelY = (1.0 / _TextureHeight);

				// Always use uv coord at start of texel to avoid center lines
				// NB: OpenGL has XY at lower left, which will be reflected in the vars
				float2 atlasPixel = float2(i.uv.x * _TextureWidth, i.uv.y * _TextureHeight);
				float2 subImagePixel = i.objectVertex;
				float2 uvInsideVoxel = float2(frac(atlasPixel.x), frac(atlasPixel.y));
				float2 uvAtlasVoxelCenter = float2((floor(atlasPixel.x) + 0.5f) / _TextureWidth, (floor(atlasPixel.y) + 0.5f) / _TextureWidth);

				fixed4 c = tex2D(_MainTex, uvAtlasVoxelCenter);

				if (i.normal.x != 0) {
					// Columns (left to right)
					if (i.extra.x == 0) {
						// Left edge
						light = 1 + lightMax - (lightDampning * _SubImageWidth);
					} else if (i.extra.x > _SubImageWidth - 0.5) {
						// Right edge
						light = 1 + lightMax;
					} else {
						// Center edges
						float2 uv_lineLeft = float2(uvAtlasVoxelCenter.x - uvOnePixelX, uvAtlasVoxelCenter.y);
						fixed4 cLeft = tex2D (_MainTex, uv_lineLeft);

						bool leftFaceIsTransparent = c.a == 0;
						bool rightFaceOnLineLeftIsTransparent = cLeft.a == 0;

						if (leftFaceIsTransparent == rightFaceOnLineLeftIsTransparent) {
							discard;
			         		return c;
						}

						if (leftFaceIsTransparent) {
							// Draw right face on line left instead
							if (!i.extra.w) {
								// Backface culling
								discard;
								return c;
							}
							c = cLeft;
							light = 1 + lightMax - (lightDampning * (_SubImageWidth - i.extra.x));
						} else {
							if (i.extra.w) {
								// Backface culling
								discard;
								return c;
							}
							light = 1 + lightMax - (lightDampning * (_SubImageWidth - i.extra.x + 10));
						}
					}
				} else if (i.normal.y != 0) {
					// Rows (bottom to top)
					if (i.extra.y == 0) {
						// Bottom edge
						light = 1 + lightMax - (lightDampning * _SubImageHeight);
					} else if (i.extra.y > _SubImageHeight - 0.5) {
						// Top edge
						light = 1 + lightMax;
					} else {
						// Center edges
						float2 uv_lineBelow = float2(uvAtlasVoxelCenter.x, uvAtlasVoxelCenter.y - uvOnePixelY);
						fixed4 cBelow = tex2D (_MainTex, uv_lineBelow);

						bool bottomFaceIsTransparent = c.a == 0;
						bool topFaceOnLineBelowIsTransparent = cBelow.a == 0;

						if (bottomFaceIsTransparent == topFaceOnLineBelowIsTransparent) {
							discard;
			         		return c;
						}

						if (bottomFaceIsTransparent) {
							// Draw top face on line below instead
							if (!i.extra.w) {
								// Backface culling
								discard;
								return c;
							}
//							i.normal *= -1;
							c = cBelow;
							light = 1 + lightMax - (lightDampning * (_SubImageHeight - i.extra.y));
						} else {
							if (i.extra.w) {
								// Backface culling
								discard;
								return c;
							}
							light = 1 + lightMax - (lightDampning * (_SubImageHeight - i.extra.y + 10));
						}
					}
				} else {
					// Front and back
//					if (i.normal.z == 1)
//						light = 1 + lightMax - (lightDampning * (_SubImageHeight + 11));

					float seam = 0.005f;
					float oneMinusSeam = 1 - 0.005f;

					if (subImagePixel.x < 0.1) {
						c = tex2D(_MainTex, float2((i.uv.x + uvOnePixelX / 4), uvAtlasVoxelCenter.y));
						c = fixed4(1,1,1,1);
					} else if (subImagePixel.x > _SubImageWidth - 1) {
						c = fixed4(1,0,0,1);
//						c = tex2D(_MainTex, float2((i.uv.x - uvOnePixelX / 4), uvAtlasVoxelCenter.y));
					}

					if (c.a == 0 && (uvInsideVoxel.x < seam || uvInsideVoxel.y < seam || uvInsideVoxel.x > oneMinusSeam || uvInsideVoxel.y > oneMinusSeam)) {
						// For transparent voxels, vi create a padding edge with colors of adjacent voxels to hide seams
						if (uvInsideVoxel.x < seam) {
							// Left line
							c = tex2D(_MainTex, float2(uvAtlasVoxelCenter.x - uvOnePixelX, uvAtlasVoxelCenter.y));
						} else if (uvInsideVoxel.x > oneMinusSeam) {
							// Right line
							c = tex2D(_MainTex, float2(uvAtlasVoxelCenter.x + uvOnePixelX, uvAtlasVoxelCenter.y));
						}

						if (c.a == 0) {
							if (uvInsideVoxel.y < seam) {
								// Bottom line (OpenGL has Y inverted!)
								c = tex2D(_MainTex, float2(uvAtlasVoxelCenter.x, uvAtlasVoxelCenter.y - uvOnePixelY));
							} else if (uvInsideVoxel.y > oneMinusSeam) {
								// Top line
								c = tex2D(_MainTex, float2(uvAtlasVoxelCenter.x, uvAtlasVoxelCenter.y + uvOnePixelY));
							}
						}

						if (c.a == 0) {
							// Check corners
							if (uvInsideVoxel.x < seam) {
								if (uvInsideVoxel.y < seam) {
									// Bottom left (OpenGL has Y inverted!)
									c = tex2D(_MainTex, float2(uvAtlasVoxelCenter.x - uvOnePixelX, uvAtlasVoxelCenter.y - uvOnePixelY));
								} else if (uvInsideVoxel.y > oneMinusSeam) {
									// Bottom right
									c = tex2D(_MainTex, float2(uvAtlasVoxelCenter.x + uvOnePixelX, uvAtlasVoxelCenter.y - uvOnePixelY));
								}
							} else if (uvInsideVoxel.x > oneMinusSeam) {
								if (uvInsideVoxel.y < seam) {
									// Bottom right (OpenGL has Y inverted!)
									c = tex2D(_MainTex, float2(uvAtlasVoxelCenter.x - uvOnePixelX, uvAtlasVoxelCenter.y + uvOnePixelY));
								} else if (uvInsideVoxel.y > oneMinusSeam) {
									// Top right
									c = tex2D(_MainTex, float2(uvAtlasVoxelCenter.x + uvOnePixelX, uvAtlasVoxelCenter.y + uvOnePixelY));
								}
							}
						}
					}
				}

#ifdef DEBUG_TEXTURE_ATLAS
				if (c.a != 1 && c.a != 0)
					c = fixed4(1, 0, 0, 1);
#endif

				if (c.a == 0) {
					// Discard transparent fragments
					discard;
				}

#ifdef USE_LIGHT
				c *= light;
//				c = DiffuseLight( i.lightDir, normal, c, LIGHT_ATTENUATION(i) );
#endif
				return c;
			}
			ENDCG
		}
	}
}
