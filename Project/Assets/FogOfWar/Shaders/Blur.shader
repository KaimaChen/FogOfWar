//参考：https://gameinstitute.qq.com/community/detail/120432

Shader "KaimaChen/Blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			ZTest Always
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				half2 taps[4] : TEXCOORD1;
			};

			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			half4 _BlurOffsets;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = o.uv - _BlurOffsets.xy * _MainTex_TexelSize.xy;

				o.taps[0] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy;
				o.taps[1] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy;
				o.taps[2] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1);
				o.taps[3] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.taps[0]);
				col += tex2D(_MainTex, i.taps[1]);
				col += tex2D(_MainTex, i.taps[2]);
				col += tex2D(_MainTex, i.taps[3]);
				return col * 0.25;
			}
			ENDCG
		}
	}
}
