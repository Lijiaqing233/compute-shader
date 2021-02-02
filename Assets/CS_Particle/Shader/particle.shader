Shader "shader test/particle"
{
	Properties
	{
		_Color("Color",color)=(1,1,1,1)
	
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass
		{
			CGPROGRAM
			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct Particle
			{
				float3 position; //vec3 = float3 = 12B
				float3 v;
				float3 a;
				float3 color;
			};

			StructuredBuffer<Particle> _ParticleBuffer;
			
			fixed4 _Color;
			float _lerp,_Size;
			float4x4 _GameobjectMatrix;
			struct appdata{
				float4 vertex:POSITION;
			};
			struct v2f{
				float4 pos:SV_POSITION;
			};
			float4x4 GetModelToWorldMatrix(float3 pos)
			{
				float4x4 transformMatrix=float4x4(
						_Size,0,0,pos.x,
						0,_Size,0,pos.y,
						0,0,_Size,pos.z,
						0,0,0,1
				);
				return transformMatrix;
			}

			v2f vert(appdata v,uint instanceID :SV_INSTANCEID)
			{
				v2f o;
				Particle particle = _ParticleBuffer[instanceID]; 
				float4x4 WorldMatrix = GetModelToWorldMatrix(particle.position.xyz);
				WorldMatrix = mul(_GameobjectMatrix,WorldMatrix);
				v.vertex = mul(WorldMatrix, v.vertex);
				o.pos = mul(UNITY_MATRIX_VP,v.vertex);
	
				return o;
			}
			fixed4 frag(v2f i):SV_Target
			{
			
				return _Color;
			}
			ENDCG
		}
		
	}
	FallBack Off
}