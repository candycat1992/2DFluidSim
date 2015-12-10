Shader "2DFluidSim/Obstacle" {
	Properties {
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

			#define ObstacleColor fixed4(0.6, 0.6, 0.6, 1.0)

			float2 _InverseSize;
			
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

			fixed4 drawCircle(float2 uv, float2 center, float radius, fixed4 color) {
				float2 diff = uv - center;
				diff.x *= _InverseSize.y/_InverseSize.x;
				float d = length(diff);
				return lerp(fixed4(0, 0, 0, 0), color, smoothstep(-0.01, 0.01, radius - d));
			}

			fixed4 drawLine(float2 uv, float2 p0, float2 p1, float width, fixed4 color) {
				float2 d0 = uv - p0;
				float2 d1 = p1 - p0;
				float h = saturate(dot(d0, d1) / dot(d1, d1));
				float2 diff = d0 - d1 * h;
				diff.x *= _InverseSize.y/_InverseSize.x;
				float d = length(diff);
				return lerp(fixed4(0, 0, 0, 0), color, smoothstep(-0.01, 0.01, 0.5 * width - d));
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 result = fixed4(0, 0, 0, 0);
				
				//draw borders
				if(i.uv.x <= _InverseSize.x) result = ObstacleColor;
				if(i.uv.x >= 1.0 - _InverseSize.x) result = ObstacleColor;
				if(i.uv.y <= _InverseSize.y) result = ObstacleColor;
				if(i.uv.y >= 1.0 - _InverseSize.y) result = ObstacleColor;

				fixed4 layer = drawCircle(i.uv, float2(0.5, 0.5), 0.1, ObstacleColor);
				result = lerp(result, layer, layer.a);

				layer = drawCircle(i.uv, float2(0.2, 0.7), 0.1, ObstacleColor);
				result = lerp(result, layer, layer.a);

				layer = drawCircle(i.uv, float2(0.8, 0.3), 0.1, ObstacleColor);
				result = lerp(result, layer, layer.a);

				float2 p0 = float2(0.2, 0.3) + 0.1 * float2(cos(_Time.y) * _InverseSize.x/_InverseSize.y, sin(_Time.y));
				float2 p1 = 2 * float2(0.2, 0.3) - p0;
				layer = drawLine(i.uv, p0, p1, 0.06, ObstacleColor);
				result = lerp(result, layer, layer.a);

				p0 = float2(0.8, 0.7) + 0.1 * float2(cos(-_Time.y) * _InverseSize.x/_InverseSize.y, sin(-_Time.y));
				p1 = 2 * float2(0.8, 0.7) - p0;
				layer = drawLine(i.uv, p0, p1, 0.06, ObstacleColor);
				result = lerp(result, layer, layer.a);
				
				return result;
			}
			ENDCG
		}
	}
}
