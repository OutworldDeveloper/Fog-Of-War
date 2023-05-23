Shader "Hidden/FowPostProcess"
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
			#include "noiseSimplex.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 viewVector : TEXCOORD1;				
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return o;
            }

            sampler2D _MainTex;
			sampler2D _TestFogText;
			sampler2D _FogTexture;
			float2 _TestOrigin;

			float2 _FowTextureRes;
			
			sampler2D _CameraDepthTexture;
			
			float rayGroundIntersect(float3 rayOrigin, float3 rayDir) 
			{
				float denom = dot(float3(0,1,0), rayDir);
				if (abs(denom) > 0.01)
				{
					// negate normal instead of rayOrigin
					float t = dot(rayOrigin, float3(0,-1,0)) / denom; 
					return t >= 0 ? t : 999999999;
				}
				return 999999999;
			}			

			float sampleFogTexture(float3 samplePoint, sampler2D fogTexture)
			{
				float2 foguv = float2(samplePoint.x, samplePoint.z) - _TestOrigin;
				foguv = foguv / _FowTextureRes;
				
				float testMultiplier = step(foguv.x, 1);
				float testMultiplier2 = step(0, foguv.x);
				float testMultiplier3 = step(foguv.y, 1);
				float testMultiplier4 = step(0, foguv.y);
				
				return tex2D(fogTexture, foguv) * min(min(testMultiplier, testMultiplier2), min(testMultiplier3, testMultiplier4));
			}
			
			float4 GetFogColor(float4 originalColor, float3 position)
			{
				float3 samplePosition = float3(position.x, _Time.y * 0.2, position.z);
				float2 noiseValue = snoise(samplePosition * 0.5);
				noiseValue = snoise(samplePosition * 0.5 + noiseValue);			
				noiseValue = (noiseValue + 1) / 2;
				
				float4 fogColor = tex2D(_FogTexture, (position.xz + noiseValue) * 0.01);
				fogColor *= 0.3;
				
				originalColor *= 0.2f;

				return originalColor + fogColor.r;
			}

            fixed4 frag (v2f i) : SV_Target
            {	
                float4 originalColor = tex2D(_MainTex, i.uv);
			
				float3 rayOrigin = _WorldSpaceCameraPos;				
				float3 rayDirection = normalize(i.viewVector);			
			
				float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float depth = LinearEyeDepth(nonLinearDepth) * length(i.viewVector);			
			
				float3 samplePoint = rayOrigin + rayDirection * rayGroundIntersect(rayOrigin, rayDirection);
				float fogValue = sampleFogTexture(samplePoint, _TestFogText);

				float4 fogColor = GetFogColor(originalColor, samplePoint);

				return lerp(fogColor, originalColor, fogValue);				
				
				//
				
				float3 worldPosition = rayOrigin + rayDirection * depth;
				float percentage = min(1, worldPosition.y / 4);
				float4 fogedColor = lerp(originalColor * 0.4, originalColor * 0.1, 1 - percentage);
				return lerp(fogedColor, originalColor, fogValue);
				//

				//return lerp(fogCol.r * 0.3, originalColor, fogValue);

				// exp

				float fogEdge = step(fogValue, 0.5) + (1 - step(fogValue, 0.51));
				fogEdge = 1 - fogEdge;
				
				fogValue = 1 - step(fogValue, 0.5);
				
                //return lerp(fogCol.r * 0.3, originalColor, fogValue) + (fogEdge * float4(1, 0, 0, 0));
            }
            ENDCG
        }
    }
}
