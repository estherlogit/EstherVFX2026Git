Shader "Workrooms/WR_SkyboxClouds_Masked"
{
    Properties
    {
        _Rotation ("Rotation", Range(0, 360)) = 0
        _TerrainTex ("Terrain Texture", 2D) = "white" {}
        _TerrainOffset ("Terrain Offset", Range(-1,1)) = 0
        _TerrainScale ("Terrain Scale", Range(0,5)) = 2
        _CloudTex ("Cloud Texture", 2D) = "white" {}
        _CloudOpacity("Cloud Opacity", Range(0,1)) = 1
        _CloudHeight("Cloud Height", Range(0,1)) = 0.5
        _ZenithColor("Zenith Color", Color) = (0,0,1,0)
        _TransitionColor("Transition Color", Color) = (0.5,0.8,1,0)
        _HorizonColor("Horizon Color", Color) = (0.8,0.8,1,0)
        _CloudTint("Cloud Tint", Color) = (1,1,1,1)
        _TransitionPos("Transition Position", Range(0.01,0.99)) = 0.1
        _CloudSpeedBack("Cloud Speed", Range(0.01,0.99)) = 0.1
        _CloudSpeedMid("Cloud Mid Speed", Range(0.01,0.99)) = 0.1
        _CloudMidOpacity("Cloud Mid Opacity", Range(0,1)) = 1
        
        _ColorTop ("Color Top", Color) = (1,1,1,1)
        _ColorBot ("Color Bot", Color) = (1,1,1,1)
        _ColorDirection ("Color Direction", Color) = (1,1,1,1)
        _ColorShadow ("Color Shadows", Color) = (1,1,1,1)   
        _ColorTint ("Tint", Color) = (1,1,1,1)
        _TopIntensity ("Top Intensity", float) = 1      
        _BotIntensity ("Bot Intensity", float) = 1     
        _DirectionIntensity ("Direction Intensity", float) = 1     
        _TopAlpha ("Top Alpha", Range(0,1)) = 0
        _BotAlpha ("Bot Alpha", Range(0,1)) = 0   
        _DirectionAlpha ("Direction Alpha", Range(0,1)) = 0   
        _Intensity ("Intensity", float) = 1   

        }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Dependencies/third_party/urp_asw_fork/universal/ShaderLibrary/core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                sampler2D _TerrainTex;
                sampler2D _CloudTex;
                half4 _CloudTex_ST;
                half4 _ZenithColor, _TransitionColor, _HorizonColor, _GroundColor, _CloudTint;
                half _TransitionPos,  _CloudOpacity, _CloudHeight, _Rotation, _TerrainOffset, _TerrainScale;
                float _CloudSpeedBack, _CloudSpeedMid, _CloudMidOpacity;
                half4 _ColorTint, _ColorTop, _ColorDirection, _ColorShadow, _ColorBot;
                float _TopIntensity, _BotIntensity, _DirectionIntensity, _Intensity, _TopAlpha, _BotAlpha, _DirectionAlpha;
            CBUFFER_END

            struct vertexInput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
             };

            struct vertexOutput {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD1;
                float2 viewAngles : TEXCOORD2;
                float2 uv : TEXCOORD0;
             };
     
             float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            {
                 float alpha = degrees * PI / 180.0;
                 float sina, cosa;
                 sincos(alpha, sina, cosa);
                 float2x2 m = float2x2(cosa, -sina, sina, cosa);
                 return float3(mul(m, vertex.xz), vertex.y).xzy;
             }

            vertexOutput vert(vertexInput input)
            {
                 vertexOutput output;

                 output.viewDir = RotateAroundYInDegrees(input.vertex, _Rotation);
                 output.viewAngles = 0;
                 // Calculate vertical view angle (angle from plane y = 0)
                 output.viewDir = normalize(output.viewDir);
                 output.viewAngles.x = asin(abs(output.viewDir.y));
                 output.viewAngles.x *= 2 * INV_PI * sign(output.viewDir.y);
                 output.viewAngles.y = abs(output.viewAngles.x);
                 
                 output.pos = TransformObjectToHClip(input.vertex);
                 output.uv = input.uv;
                 return output;
             }

            half4 frag(vertexOutput input) : COLOR
            {
                //Calculate horizontal view angle
                float viewXZAngle = atan2(input.viewDir.z, input.viewDir.x);
                viewXZAngle *= -INV_PI;

                float2 terrainUV = float2(viewXZAngle, input.viewAngles.x);
                

                half4 skyCol = lerp(lerp(_HorizonColor, _TransitionColor, input.viewDir.y/_TransitionPos), lerp(_TransitionColor, _ZenithColor, (input.viewDir.y - _TransitionPos)/(1.0 - _TransitionPos)), step(_TransitionPos, input.viewDir.y));
                half4 skyColMirrored = lerp(lerp(_HorizonColor, _TransitionColor, -input.viewDir.y/_TransitionPos), lerp(_TransitionColor, _ZenithColor, (-input.viewDir.y - _TransitionPos)/(1.0 - _TransitionPos)), step(_TransitionPos, -input.viewDir.y));

                float2 cloudUV = half2(viewXZAngle - 0.46, input.viewAngles.y) * _CloudTex_ST.xy + _CloudTex_ST.zw;
                cloudUV.x = (cloudUV.x * .5) + 0.5 * _Time * _CloudSpeedBack;
                half4 clouds = tex2D(_CloudTex, cloudUV);
                clouds.a *= _CloudOpacity;
                clouds.rgb *= _CloudTint;

                terrainUV.x = (terrainUV.x * .5) + 0.5;
                
                terrainUV.y += _TerrainOffset;
                terrainUV.y *= _TerrainScale;
                
                //Sample & mix terrain
                half4 mirroredSky = lerp(skyCol, skyColMirrored, step(input.uv.y, 0)) + clouds * _CloudOpacity;

                half4 terrain = tex2D(_TerrainTex, terrainUV);

                half4 colorTop = lerp(_ColorShadow, _ColorTop, terrain.r) * _TopIntensity;    
                half4 colorBot = lerp(colorTop, _ColorBot, terrain.g) * _BotIntensity;  
                half4 colorDirection = lerp(colorBot, _ColorDirection, terrain.b) * _TopIntensity;   

                half4 color = saturate(colorDirection) * _Intensity;
                terrain.a *= _CloudMidOpacity;
                float alpha = terrain.a - colorTop.r * _TopAlpha;
                alpha = alpha - colorTop.g * _BotAlpha;
                alpha = alpha - colorTop.b * _DirectionAlpha;
                alpha = saturate(alpha);
                
                half4 finalColor = lerp(mirroredSky, color, alpha);
                finalColor.a = 1.0;
                
                return finalColor;
            }
            ENDHLSL
            }
        }
    }