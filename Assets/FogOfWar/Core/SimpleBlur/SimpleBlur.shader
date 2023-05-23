Shader "Hidden/SimpleBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
				float3 blurSample = 
					tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, -1)) +
					tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, 1)) +
					tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, -1)) +
					tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, 1));
								
                return fixed4(blurSample * 0.25, 1);
            }
            ENDCG
        }
		
        Pass
        {
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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
				float3 blurSample = 
					tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-0.5, -0.5)) +
					tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-0.5, 0.5)) +
					tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(0.5, -0.5)) +
					tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(0.5, 0.5));
								
                return fixed4(blurSample * 0.25, 1);
            }
            ENDCG
        }		
		
    }
}
