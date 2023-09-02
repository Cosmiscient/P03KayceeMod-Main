Shader "P03/Projector/StickerStencilApply"
{
	SubShader 
	{
		Tags 
		{ 
			"RenderType"="Opaque"
			"Queue"="AlphaTest" 
		}
		
		Pass
		{
			ColorMask 0
		
			Stencil 
			{
				Ref 1
				Comp Always
				Pass Replace
			}
		
			CGINCLUDE
			#include "UnityCG.cginc"
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
			};
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				return half4(0f, 0f, 0f, 0f);
			}
			
			ENDCG
		}
	}
}
