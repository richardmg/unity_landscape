Shader "Tree billboard shader" {
   Properties {
      _MainTex ("Texture Image", 2D) = "white" {}
   }
   SubShader {
      Tags { "RenderType" = "Transparent" }

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
            vertexOutput output;
            output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
            output.tex = input.tex;
            output.depth = 0;

            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR
         {
         	float4 rgba = tex2D(_MainTex, float2(input.tex.xy));
         	if (rgba.x == 0 && rgba.y == 0 && rgba.z == 0)
         		rgba.a = 0;
            return rgba; 
         }
 
         ENDCG
      }
   }
}