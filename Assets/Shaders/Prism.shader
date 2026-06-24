Shader "Custom/Prism"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Prism controls
        _PrismStrength ("Prism Strength", Range(0, 2)) = 1.2
        _ScrollSpeed ("Scroll Speed", Range(0, 10)) = 2.0
        _LineDensity ("Line Density", Range(1, 30)) = 8.0
        _LineSharpness ("Line Sharpness", Range(1, 20)) = 6.0
        _Brightness ("Brightness Boost", Range(0.5, 3)) = 1.4
        
        [Toggle] _UseWorldPos ("Use World Position", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _PrismStrength;
            float _ScrollSpeed;
            float _LineDensity;
            float _LineSharpness;
            float _Brightness;
            float _UseWorldPos;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                
                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            // Horizontal rainbow spectrum
            fixed3 HorizontalSpectrum(float y, float time)
            {
                float phase = (y * _LineDensity) + (time * _ScrollSpeed);
                float hue = frac(phase);
                
                // Smooth rainbow gradient
                float3 rainbow = saturate(0.5 + 0.5 * cos(6.28318 * (hue + float3(0.0, 0.33, 0.67))));
                return rainbow;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 c = tex * i.color;
                
                // Get vertical position for the lines
                float verticalPos;
                if (_UseWorldPos > 0.5)
                    verticalPos = i.worldPos.y;
                else
                    verticalPos = i.uv.y;
                
                float3 prism = HorizontalSpectrum(verticalPos, _Time.y);
                
                // Sharpen the lines
                float sharpness = pow(0.5 + 0.5 * sin(6.28318 * (verticalPos * _LineDensity + _Time.y * _ScrollSpeed)), _LineSharpness);
                
                // Apply effect
                c.rgb = lerp(c.rgb, c.rgb * prism * _Brightness, _PrismStrength * sharpness);
                
                // Extra glow on bright areas
                float lum = dot(c.rgb, float3(0.3, 0.59, 0.11));
                c.rgb += prism * lum * _PrismStrength * 0.25;
                
                return c;
            }
            ENDCG
        }
    }
}