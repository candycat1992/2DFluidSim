Shader "2DFluidSim/DrawTransparent" {
	Properties {
		_MainTex ("_MainTex", 2D) = "black" {}
		_ObstacleTex ("_ObstacleTex", 2D) = "black" {}
		_Transparancy ("_Transparancy", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 100

		Pass {
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _ObstacleTex;
			float _Transparancy;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 obs = tex2D(_ObstacleTex, i.uv);	
				fixed a = col.r * 0.299 + col.g * 0.587 + col.b * 0.114;
				return fixed4(lerp(col.rgb, obs.rgb, obs.a), a * _Transparancy + obs.a);
			}
			ENDCG
		}
	}
}
