Shader "Custom/VoxelObjectVolume"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DetailTex ("Detail Texture", 2D) = "white" {}
		_BaseLight ("Base light", Range(0, 2)) = 0.85
		_AmbientLight ("Ambient", Range(0, 2)) = 1.1
		_Sunshine ("Sunshine", Range(0, 3)) = 1.6
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
			Cull Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			// Should perhaps split this into three shaders:
			// 1. exact, but without culling (for zero depth faces)
			// 2. exact, but without shadow and lightning (for mid-range?)
			// 3. volume, without shadow and lightning (for far-distance)
			#define NO_LIGHT
			#define NO_SELF_SHADOW
			#include "VoxelObject.cginc"

			ENDCG
		}
	}
}
