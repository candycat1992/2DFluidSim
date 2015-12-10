Shader "2DFluidSim/SubtractGradient" {
	Properties {
		_VelocityTex ("_VelocityTex", 2D) = "white" {}
		_PressureTex ("_VelocityTex", 2D) = "white" {}
		_ObstacleTex ("_VelocityTex", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {
			ZTest Always
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D _VelocityTex;
			sampler2D _PressureTex;
			float2 _PressureTex_TexelSize;
			sampler2D _ObstacleTex;
			float2 _ObstacleTex_TexelSize;
			
			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				float pL = tex2D(_PressureTex, i.uv + half2(-1, 0) * _PressureTex_TexelSize).x;
				float pR = tex2D(_PressureTex, i.uv + half2(1, 0) * _PressureTex_TexelSize).x;
				float pT = tex2D(_PressureTex, i.uv + half2(0, 1) * _PressureTex_TexelSize).x;
				float pB = tex2D(_PressureTex, i.uv + half2(0, -1) * _PressureTex_TexelSize).x;
				float pC = tex2D(_PressureTex, i.uv).x;
				
				// Find neighboring obstacles
			    float oL = tex2D(_ObstacleTex, i.uv + half2(-1, 0) * _ObstacleTex_TexelSize).a;
			    float oR = tex2D(_ObstacleTex, i.uv + half2(1, 0) * _ObstacleTex_TexelSize).a;
			    float oT = tex2D(_ObstacleTex, i.uv + half2(0, 1) * _ObstacleTex_TexelSize).a;
			    float oB = tex2D(_ObstacleTex, i.uv + half2(0, -1) * _ObstacleTex_TexelSize).a;
				
				if(oL > 0.9) pL = pC;
			    if(oR > 0.9) pR = pC;
			    if(oT > 0.9) pT = pC;
			    if(oB > 0.9) pB = pC;
			
				float2 u = tex2D(_VelocityTex, i.uv).xy;
				u = u - 0.5 * float2(pR - pL, pT- pB);
			    
			    return float4(u, 0, 1);
			}
			ENDCG
		}
	}
}
