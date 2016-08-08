Shader "Custom/VoxelObjectLod0"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
		_Gradient ("Gradient", Range(0, 0.6)) = 0.3
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

			#pragma vertex vert
			#pragma fragment frag

			#define USE_LOD0

			#include "VoxelObject.cginc"

			v2f vert(appdata v)
			{
				return voxelobject_vert(v);
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				return voxelobject_frag(i);
			}

			ENDCG
		}
	}
}
