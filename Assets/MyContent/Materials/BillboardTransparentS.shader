Shader "Custom/Billboard transparent" {
   Properties {
      _MainTex ("Texture Image", 2D) = "white" {}
   }
   SubShader {
      Tags {
          "RenderType" = "TreeBillboard"
          // Note that Queue can be overridden by the material (custom queue). 
          "Queue" = "Transparent"
          "DisableBatching" = "True"
      }

      Pass {
      	 ZWrite Off
      	 Blend SrcAlpha OneMinusSrcAlpha
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

            // Note that we use + for quad and - for cube in
            float scaleX = length(mul(_Object2World, float4(1.0, 0.0, 0.0, 0.0)));
            float scaleY = length(mul(_Object2World, float4(0.0, 1.0, 0.0, 0.0)));
            output.pos = mul(UNITY_MATRIX_P, 
              mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
              + float4(input.vertex.x * scaleX, input.vertex.y * scaleY, 0.0, 0.0));

            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR
         {
         	float4 rgba = tex2D(_MainTex, float2(input.tex.xy));
            return rgba; 
         }
 
         ENDCG
      }
   }
}