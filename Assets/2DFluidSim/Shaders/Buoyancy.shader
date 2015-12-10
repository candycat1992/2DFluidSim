Shader "2DFluidSim/Buoyancy" {
	Properties {
		_VelocityTex ("_VelocityTex", 2D) = "white" {}
		_TemperatureTex ("_TemperatureTex", 2D) = "white" {}
		_DensityTex ("_DensityTex", 2D) = "white" {}
		_AmbientTemperature ("_AmbientTemperature", Float) = 0.0
		_DeltaT ("_DeltaT", Float) = 0.0
		_Sigma ("_Sigma", Float) = 0.0
		_Kappa ("_Kappa", Float) = 0.0
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
			sampler2D _TemperatureTex;
			sampler2D _DensityTex;
			float _AmbientTemperature;
			float _DeltaT;
			float _Sigma;
			float _Kappa;
			
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
				float2 u = tex2D(_VelocityTex, i.uv).xy;
				float d = tex2D(_DensityTex, i.uv).x;
				float t = tex2D(_TemperatureTex, i.uv).x;
				
				if (t > _AmbientTemperature) {
					u += (-_Kappa * d + _DeltaT * _Sigma * (t - _AmbientTemperature)) * float2(0, 1);
				}
				
				return fixed4(u, 0, 1);
			}
			ENDCG
		}
	}
}
