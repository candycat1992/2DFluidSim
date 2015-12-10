Shader "2DFluidSim/Jacobi2d" {
	Properties {
		_XTex ("_XTex", 2D) = "white" {}
		_BTex ("_BTex", 2D) = "white" {}
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
			
			sampler2D _XTex;
			float2 _XTex_TexelSize;
			sampler2D _BTex;
			sampler2D _ObstacleTex;
			float2 _ObstacleTex_TexelSize;
			
			float _Alpha;
			float _rBeta;
			
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
				float2 xL = tex2D(_XTex, i.uv + half2(-1, 0) * _XTex_TexelSize).xy;
				float2 xR = tex2D(_XTex, i.uv + half2(1, 0) * _XTex_TexelSize).xy;
				float2 xT = tex2D(_XTex, i.uv + half2(0, 1) * _XTex_TexelSize).xy;
				float2 xB = tex2D(_XTex, i.uv + half2(0, -1) * _XTex_TexelSize).xy;
				float2 xC = tex2D(_XTex, i.uv).xy;
				
				// Find neighboring obstacles
			    float oL = tex2D(_ObstacleTex, i.uv + half2(-1, 0) * _ObstacleTex_TexelSize).a;
			    float oR = tex2D(_ObstacleTex, i.uv + half2(1, 0) * _ObstacleTex_TexelSize).a;
			    float oT = tex2D(_ObstacleTex, i.uv + half2(0, 1) * _ObstacleTex_TexelSize).a;
			    float oB = tex2D(_ObstacleTex, i.uv + half2(0, -1) * _ObstacleTex_TexelSize).a;
				
				if(oL > 0.9) xL = xC;
			    if(oR > 0.9) xR = xC;
			    if(oT > 0.9) xT = xC;
			    if(oB > 0.9) xB = xC;
			
				float2 bC = tex2D(_BTex, i.uv).xy;
				
			    float2 result = (xL + xR + xT + xB + _Alpha * bC) * _rBeta;
			    
			    return float4(result, 0, 1);
			}
			ENDCG
		}
	}
}
