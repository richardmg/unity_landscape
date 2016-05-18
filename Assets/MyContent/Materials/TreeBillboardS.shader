Shader "Tree billboard shader" {
   Properties {
      _MainTex ("Texture Image", 2D) = "white" {}
   }
   SubShader {
      Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

      Blend SrcAlpha OneMinusSrcAlpha
      ZWrite Off

      Pass {

      	 Cull off

         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 

         // User-specified uniforms            
         uniform sampler2D _MainTex;        
 
         struct vertexInput {
            float4 vertex : POSITION;
            float4 tex : TEXCOORD0;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float depth : SV_Depth;
            float4 tex : TEXCOORD0;
         };
 
         vertexOutput vert(vertexInput input) 
         {
//         	#if UNITY_UV_STARTS_AT_TOP
//        		input.vertex.y = 1 - input.vertex.y;
//			#endif

            vertexOutput output;
//            output.pos = input.vertex;

            output.pos = mul(UNITY_MATRIX_MVP, input.vertex);

//            output.pos = mul(UNITY_MATRIX_P, 
//              mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
//              - float4(input.vertex.x, input.vertex.y, 0.0, 0.0));
 
            output.tex = input.tex;
            output.depth = 0;

            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR
         {
         	float4 rgba = tex2D(_MainTex, float2(input.tex.xy));
         	if (rgba.x == 0 && rgba.y == 0 && rgba.z == 0)
         		rgba[3] = 0;
            return rgba; 
         }
 
         ENDCG
      }
   }
}