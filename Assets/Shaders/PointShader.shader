// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/PointShader"
{
	Properties
	{
		_Color("Color", Color) = (1,0,0,1)
	}
		SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			float posScale = 1.0f;
			struct Data{
				float3 pos;
			};

			//The buffer containg the points, we want to draw
			StructuredBuffer<Data> buf_points;
			float3 _worldPos;
			float4 _Color;

			struct v2f
			{
				float4 pos : POSITION;
				float3 color : COLOR0;
			}; 

			//fetchs a point from the buffer corresponding to the vertex index
			v2f vert (uint id : SV_VertexID )
			{
				v2f o;
				float3 scaledPointPos = float3(buf_points[id].pos.x * posScale, buf_points[id].pos.y, buf_points[id].pos.z * posScale);
				float3 worldPos = scaledPointPos + _worldPos;
				o.pos = UnityObjectToClipPos(float4 (worldPos,1.0f));
				o.color  = worldPos; 
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{  
				float4 c = float4(i.color,1.0f);
				c = saturate(c);
				return c;
			}
			ENDCG
		}
	}
}
