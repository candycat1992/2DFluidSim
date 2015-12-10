Shader "2DFluidSim/Advect" {
	Properties {
		_SrcTex ("_SrcTex", 2D) = "white" {}
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
			
			sampler2D _SrcTex;
			float2 _SrcTex_TexelSize;
			sampler2D _VelocityTex;
			sampler2D _ObstacleTex;
			float _DeltaT;
			
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
				float2 coord = i.uv - u * _DeltaT	* _SrcTex_TexelSize;
				float4 result = tex2D(_SrcTex, coord);
				
				float solid = tex2D(_ObstacleTex, i.uv).a;
			    if (solid > 0.9) {
			    	result = float4(0, 0, 0, 0);
			    }
				
				return result;
			}
			ENDCG
		}
	}
}
