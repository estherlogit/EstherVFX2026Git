Shader "Custom/Cloud Flow Map"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ColorTop ("Color Top", Color) = (1,1,1,1)
        _ColorBot ("Color Bot", Color) = (1,1,1,1)
        _ColorRim ("Color Rim", Color) = (1,1,1,1)
        _ColorShadow ("Color Shadows", Color) = (1,1,1,1)   
        _ColorTint ("Tint", Color) = (1,1,1,1)
        _TopIntensity ("Top Intensity", float) = 1      
        _BotIntensity ("Bot Intensity", float) = 1     
        _RimIntensity ("Rim Intensity", float) = 1     
        _Intensity ("Intensity", float) = 1   
        
        _Alpha ("Alpha", float) = 1   

        _FlowMap ("Flow Map", 2D) = "white" {}
        _FlowDistortion ("Flow Distortion", float) = 0.05
        _FlowSpeed ("Flow Speed", float) = 1   
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
            };
            
            fixed4 _ColorTint;
            fixed4 _ColorBot;
            fixed4 _ColorTop;
            fixed4 _ColorRim;
            fixed4 _ColorShadow;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _FlowMap;
            float _FlowDistortion;
            float _FlowSpeed;
            float _TopIntensity;
            float _BotIntensity;
            float _RimIntensity;
            float _Intensity;
            float _Alpha;

            

            fixed4 frag(v2f IN) : SV_Target
            {
                float3 flowDir = tex2D(_FlowMap, IN.texcoord) * 2.0f - 1.0f ;
                flowDir *= _FlowDistortion * -1;

                float phase0 = frac(_Time[1] * _FlowSpeed * 0.5f + 0.5f);
                float phase1 = frac(_Time[1] * _FlowSpeed * 0.5f + 1.0f);
                float flowLerp = abs((0.5f - phase0) / 0.5f);
                
                half4 tex0 = tex2D(_MainTex, IN.texcoord + flowDir.xy * phase0);
                half4 tex1 = tex2D(_MainTex, IN.texcoord + flowDir.xy * phase1);       
                half4 colorAlpha = lerp(tex0, tex1, flowLerp);
                
                half4 colorTop = lerp(_ColorShadow, _ColorTop, colorAlpha.r) * _TopIntensity;    
                half4 colorBot = lerp(_ColorShadow, _ColorBot, colorAlpha.g) * _BotIntensity;  
                half4 colorRim = lerp(_ColorShadow, _ColorRim, colorAlpha.b) * _RimIntensity;  

                half4 color = saturate((colorTop + colorBot + colorRim) * _Intensity);

                float alpha = colorAlpha.a * _Alpha;  
                half4 final = half4(color.r, color.g, color.b, alpha);

                fixed4 c = final;
                c *= _ColorTint;
                
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}