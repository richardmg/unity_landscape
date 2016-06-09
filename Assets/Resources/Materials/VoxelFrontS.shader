Shader "Custom/VoxelFront" {
   Properties {
      _MainTex ("Texture Image", 2D) = "white" {}
   }
   SubShader {
      Tags {
          "RenderType" = "TreeBillboard"
          "Queue" = "AlphaTest"
      }

//      Pass {
//          Tags { 
//              "LightMode" = "ShadowCaster"
//          }
//      	  AlphaTest Greater [_CutOffShadow]
//     	  SetTexture [_MainTex]
//      }

      Pass {
      	 Cull Back

         CGPROGRAM

         #define DEBUG_TEXTURE_ATLAS

         #pragma vertex vert  
         #pragma fragment frag 

         // User-specified uniforms            
         uniform sampler2D _MainTex;        

         struct vertexInput {
            float4 vertex : POSITION;
            float4 uv : TEXCOORD0;
            float4 uv2 : TEXCOORD1;
         };

         struct vertexOutput {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
            float2 uv2 : TEXCOORD1;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
            output.uv = input.uv;
            output.uv2 = input.uv2;
            output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR
         {
			float4 c = tex2D(_MainTex, input.uv);

#ifdef DEBUG_TEXTURE_ATLAS
			if (c.a != 1 && c.a != 0)
				c = fixed4(1, 0, 0, 1);
#endif

         	if (c.a < 1)
         		discard;

            return c; 
         }
 
         ENDCG
      }

   }
}