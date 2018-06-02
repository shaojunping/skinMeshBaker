/*
Created by jiadong chen
http://www.chenjd.me
*/

Shader "chenjd/AnimMapShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AnimMap ("AnimMap", 2D) ="white" {}
		_AnimLen("Anim Length", Float) = 0
		_MinPos("Min Pos", Vector) = (0.0, 0, 0, 0)
		_MaxPos("Max Pos", Vector) = (0.0, 0, 0, 0)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100
			Cull off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//开启gpu instancing
			#pragma multi_compile_instancing


			#include "UnityCG.cginc"
			#pragma target 3.0

			struct appdata
			{
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _AnimMap;
			float4 _AnimMap_TexelSize;//x == 1/width

			float4 _MinPos;
			float4 _MaxPos;

			float _AnimLen;

			
			v2f vert (appdata v, uint vid : SV_VertexID)
			{
				UNITY_SETUP_INSTANCE_ID(v);

				float f = _Time.y / _AnimLen;

				fmod(f, 1.0);

				float animMap_x = (vid + 0.5) * _AnimMap_TexelSize.x;
				float animMap_y = f;

				//这里贴图里包含的坐标而不是普通的贴图，所以要用lod0
				float4 pos = tex2Dlod(_AnimMap, float4(animMap_x, animMap_y, 0, 0));
				float3 diff = float3(0, 0, 0);
				diff.x = (_MaxPos.x - _MinPos.x) * pos.x;
				diff.y = (_MaxPos.y - _MinPos.y) * pos.y;
				diff.z = (_MaxPos.z - _MinPos.z) * pos.z;
				pos.xyz = _MinPos.xyz + diff;

				v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vertex = UnityObjectToClipPos(pos);
				/*v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);*/
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
