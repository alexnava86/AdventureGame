Shader "Custom/Rainbow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Prismatic controls
        _PrismStrength ("Prism Strength", Range(0, 2)) = 0.8
        _PrismSpeed ("Prism Speed", Range(0, 5)) = 1.5
        _PrismScale ("Prism Scale", Range(0.1, 10)) = 3.0
        _Brightness ("Brightness", Range(0, 2)) = 1.2
        
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
            float _PrismSpeed;
            float _PrismScale;
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

            // Simple rainbow function based on angle + time
            fixed3 PrismColor(float2 pos, float time)
            {
                float angle = atan2(pos.y, pos.x);
                float hue = (angle * _PrismScale * 0.15915) + (time * _PrismSpeed); // 1/(2π)
                hue = frac(hue);
                
                // Rainbow gradient (HSV to RGB)
                float3 p = abs(frac(hue + float3(0.0, 0.666, 0.333)) * 6.0 - 3.0);
                return saturate(float3(1.0, 1.0, 1.0) - abs(p - 1.0));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 c = tex * i.color;
                
                // Choose position for prism effect
                float2 prismPos = _UseWorldPos > 0.5 ? i.worldPos.xy : i.uv * 2.0 - 1.0;
                
                float3 prism = PrismColor(prismPos, _Time.y);
                
                // Blend prism with original color
                c.rgb = lerp(c.rgb, c.rgb * prism * _Brightness, _PrismStrength);
                
                // Optional: stronger effect on brighter pixels
                float luminance = dot(c.rgb, float3(0.299, 0.587, 0.114));
                c.rgb += prism * luminance * _PrismStrength * 0.3;
                
                return c;
            }
            ENDCG
        }
    }
}