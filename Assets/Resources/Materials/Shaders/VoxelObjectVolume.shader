Shader "Custom/VoxelObjectVolume"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Stripes ("Stripes", Range(0, 0.1)) = 0.05
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
