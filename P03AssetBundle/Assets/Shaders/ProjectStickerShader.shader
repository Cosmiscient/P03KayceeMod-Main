﻿Shader "P03/Projector/StenciledSticker" {
	Properties {
		_ShadowTex ("Cookie", 2D) = "white" {}
		_FalloffTex ("FallOff", 2D) = "white" {}
		_ClipTex ("Clipper", 2D) = "white" {}
	}
	Subshader {
		Tags {"Queue"="Transparent"}
		
		Stencil {
			Ref 1
			Comp Equal
		}
		
		Pass {
		
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
		   
			struct v2f {
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
			};
		   
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
		   
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.uvShadow = mul (unity_Projector, vertex);
				o.uvFalloff = mul (unity_ProjectorClip, vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
		   
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;
			sampler2D _ClipTex;
		   
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texS = tex2Dproj (_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
				fixed4 texF = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
				fixed4 texC = tex2Dproj (_ClipTex, UNITY_PROJ_COORD(i.uvShadow));
 
				fixed4 res = texS;
				res.a *= texF.a;
				res.a *= texC.a;
 
				UNITY_APPLY_FOG(i.fogCoord, res);
				return res;
			}
			ENDCG
		}
	}
}
