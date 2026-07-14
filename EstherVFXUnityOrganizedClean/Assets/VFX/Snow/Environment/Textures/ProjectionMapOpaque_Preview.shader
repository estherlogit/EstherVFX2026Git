Shader "WITHIN/Environmental/Projection Map Opaque_Preview"
{
    Properties
    {
		_CubeMap("CubeMap", Cube) = "_Skybox" {}

		[Header(Equator)]
		_EquatorOffset("Equator Offset", Range(-0.1,0.1)) = 0
		_MinEquatorOffsetInfluenceRadius("Min. Equator Offset Influence Radius", Range(0, 1200)) = 0

		[Header(Rotate about YAxis)]
		_YawRotationDeg("Yaw Rotation (Deg.)", Range(-180,180)) = 0
	}
    SubShader
    {
        Tags
        {
            "IgnoreProjector"="True"
            "Queue"="Geometry"
            "RenderType"="Opaque"
        }
        Pass
        {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			#pragma target 5.0
			#pragma multi_compile _ SN_HDR_LIGHTING


			uniform samplerCUBE _CubeMap;
			float _EquatorOffset;
			float _MinEquatorOffsetInfluenceRadius;
			float _YawRotationDeg;

			struct VertexInput
            {
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct VertexOutput
            {
				half4   pos      : SV_POSITION;
				float3  posWorld : TEXCOORD0;
				float3  worldOriginPos : TEXCOORD1;		// object origin in world coordinates
				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput vert(VertexInput v)
            {
				VertexOutput o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.posWorld = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f));

				o.pos = mul(UNITY_MATRIX_VP, float4(o.posWorld, 1.0f));

				o.worldOriginPos = mul(unity_ObjectToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;

				return o;
			}

			float2 Rotate2D(float2 xy, float angleDeg)
			{
				float cosYawDeg;
				float sinYawDeg;
				sincos(radians(angleDeg), sinYawDeg, cosYawDeg);

				// Note: 2x2 matrix multiply: v'=Mv
				//float2x2 rotationMatrix = float2x2(cosYawDeg, -sinYawDeg, sinYawDeg, cosYawDeg);
				//return mul(rotationMatrix, xy);

				// Note: Replace the 2x2 matrix multiply with 2 dot products.
				return float2(dot(float2(cosYawDeg, -sinYawDeg), xy), dot(float2(sinYawDeg, cosYawDeg), xy));
			}

			fixed4 frag(VertexOutput i) : COLOR
            {
				//return float4(_YawRotationDeg/30, _YawRotationDeg/30, _YawRotationDeg/30, 1.)

                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				// Adjust the projection height offset but only for pixels which are outside a given radius.
				// In typical use, a mesh which consists of close-up mesh detail that is also part of the sky sphere,
				// use the min. projection radius to exclude these pixel from the offset.
				float distToCamera = length(i.posWorld - _WorldSpaceCameraPos.xyz);
				float minInfluence = step(_MinEquatorOffsetInfluenceRadius, distToCamera);
				float equatorOffset = lerp(0.0f, _EquatorOffset, minInfluence);

				// Compute the world-space direction from the object's origin to the current pixel's world position. Manually tweak the direction with an equator offset.
				float3 direction = normalize(i.posWorld.xyz - i.worldOriginPos) + float3(0.0f, equatorOffset, 0.0f);

				// Rotate the view direction about the  y-axis.
				float yawRotationDeg = lerp(0.0f, _YawRotationDeg, minInfluence);
				float2 rotatedXZ = Rotate2D(direction.xz, yawRotationDeg);

				// Sample the cubemap with the tweaked direction.
				fixed3 mapTex = texCUBEbias(_CubeMap, float4(rotatedXZ.x, direction.y, rotatedXZ.y, -0.5f));
//#if SN_HDR_LIGHTING
//				mapTex = ApplyTonemapping(mapTex);
//#endif
				return fixed4(mapTex, 1.0f);
			}
			ENDCG
        }

    }
}
