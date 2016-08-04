Shader "Custom/VoxelVolumePostProcessS" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_bwBlend ("Black & White blend", Range (0, 1)) = 0
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float _bwBlend;

			float4 frag(v2f_img i) : COLOR {
				float4 c = tex2D(_MainTex, i.uv);
				if (c.a < 1) c = float4(1, 0, 0, c.a);

				return c;
			}
			ENDCG
		}
	}
}