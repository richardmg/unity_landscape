Shader "Custom/Billboard cutoff movement" {
   Properties {
      _MainTex ("Texture Image", 2D) = "white" {}
      _CutOff("Cut off", Range(0,1)) = 0.8
   }
   SubShader {
      Tags {
          "RenderType" = "TreeBillboard"
          "Queue" = "AlphaTest"
          "DisableBatching" = "True"
      }

      Pass {
      	 Cull Off

         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 

         // User-specified uniforms            
         uniform sampler2D _MainTex;        
		 uniform float _CutOff; 

         struct vertexInput {
            float4 vertex : POSITION;
            float4 tex : TEXCOORD0;
         };

         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 tex : TEXCOORD0;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
            output.tex = input.tex;

	        float4 modelView = mul(UNITY_MATRIX_MV, float4(0,0,0,1));
	        float4 inPos = input.position;
	 
	        float2 r1 = float2(_Object2World[0][0],_Object2World[0][2]);
	        float2 r2 = float2(_Object2World[2][0],_Object2World[2][2]);
	        float2 inPos0 = inPos.x * r1;
	 
	        inPos0 += inPos.z * r2;
	        inPos.xy = inPos0;
	        inPos.z = 0.0;
	        inPos.xyz += modelView.xyz;
	
            output.position = mul(UNITY_MATRIX_P, inPos);
	 
            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR
         {
         	float4 rgba = tex2D(_MainTex, float2(input.tex.xy));
         	if (rgba.a < _CutOff)
         		discard;
            return rgba; 
         }
 
         ENDCG
      }
   }
}