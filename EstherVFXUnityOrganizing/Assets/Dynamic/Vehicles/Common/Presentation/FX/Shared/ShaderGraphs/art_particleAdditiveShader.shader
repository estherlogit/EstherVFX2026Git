Shader "King/Particles/Particle Additive" {
    Properties {
        [SurfaceType(_SrcBlend, _DstBlend, _ZWrite)] _Surface("Surface Type", Vector) = (0, 0, 0, 0)
        [Enum(RGB,14,RGBA,15)] _ColorWriteMask("ColorWriteMask", Float) = 15

        _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Boost ("Boost", float) = 1

        [HideInInspector] _SrcBlend("Source Blend", Float) = 0.0
        [HideInInspector] _DstBlend("Destination Blend", Float) = 0.0
        [HideInInspector] _ZWrite("Z-Write", Float) = 1.0
    }

    SubShader {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "Queue" = "Transparent"
            "PreviewType"="Plane"
        }

        Pass {
            Name "ForwardLit"

            ColorMask[_ColorWriteMask]
            Blend  [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            half4 _TintColor;
            float _Boost;
            float4 _MainTex_ST;
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct appdata_t {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;

                #ifdef SOFTPARTICLES_ON
                float4 projPos : TEXCOORD2;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.color = v.color * _TintColor;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                half4 col = i.color * tex * _Boost;

                // I don't understand the following comment from the original
                // shader, but I still keep it for reference:
                // alpha should not have double-brightness applied to it,
                // but we can't fix that legacy behavior without breaking
                // everyone's effects, so instead clamp the output to get
                // sensible HDR behavior (case 967476)
                col.a = saturate(col.a);

                return col;
            }
            ENDHLSL
        }
    }
}
