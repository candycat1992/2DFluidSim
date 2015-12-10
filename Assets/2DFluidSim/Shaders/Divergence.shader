Shader "2DFluidSim/Divergence" {
	Properties {
		_VelocityTex ("_VelocityTex", 2D) = "white" {}
		_ObstacleTex ("_ObstacleTex", 2D) = "white" {}
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
			float2 _VelocityTex_TexelSize;
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
				float2 uL = tex2D(_VelocityTex, i.uv + half2(-1, 0) * _VelocityTex_TexelSize).xy;
				float2 uR = tex2D(_VelocityTex, i.uv + half2(1, 0) * _VelocityTex_TexelSize).xy;
				float2 uT = tex2D(_VelocityTex, i.uv + half2(0, 1) * _VelocityTex_TexelSize).xy;
				float2 uB = tex2D(_VelocityTex, i.uv + half2(0, -1) * _VelocityTex_TexelSize).xy;
				float2 uC = tex2D(_VelocityTex, i.uv).xy;
				
				// Find neighboring obstacles
			    float oL = tex2D(_ObstacleTex, i.uv + half2(-1, 0) * _ObstacleTex_TexelSize).a;
			    float oR = tex2D(_ObstacleTex, i.uv + half2(1, 0) * _ObstacleTex_TexelSize).a;
			    float oT = tex2D(_ObstacleTex, i.uv + half2(0, 1) * _ObstacleTex_TexelSize).a;
			    float oB = tex2D(_ObstacleTex, i.uv + half2(0, -1) * _ObstacleTex_TexelSize).a;
				
				if(oL > 0.9) uL = 0.0;
			    if(oR > 0.9) uR = 0.0;
			    if(oT > 0.9) uT = 0.0;
			    if(oB > 0.9) uB = 0.0;
			
			    float result = 0.5 * (uR.x - uL.x + uT.y - uB.y);
			    
			    return float4(result, 0, 0, 1);
			}
			ENDCG
		}
	}
}
