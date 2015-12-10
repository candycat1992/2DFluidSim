Shader "2DFluidSim/Fade" {
	Properties {
		_SrcTex ("_SrcTex", 2D) = "white" {}
		_FadeSpeed ("_FadeSpeed", Float) = 0.0
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
			
			sampler2D _SrcTex;
			float _FadeSpeed;
			
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
			    float3 result = tex2D(_SrcTex, i.uv).xyz;
			    result = result * _FadeSpeed;
			    
			    return float4(result, 1);
			}
			ENDCG
		}
	}
}
