Shader "Custom/SmokyFogDistortion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (Skulls)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Distortion
        _DistortStrength ("Distort Strength", Range(0, 0.3)) = 0.09
        _DistortSpeed ("Distort Speed", Range(0, 5)) = 1.2
        _DistortScale ("Distort Scale", Range(0.5, 12)) = 4.5
        
        // Flow
        _FlowSpeed ("Upward Flow Speed", Range(-1, 3)) = 0.7
        _Turbulence ("Turbulence", Range(0, 3)) = 1.1
        
        // Appearance
        _Softness ("Radial Softness", Range(0.1, 5)) = 2.2
        _FadeAmount ("Fade/Disperse", Range(0, 1)) = 0.0
        _Intensity ("Glow Intensity", Range(0, 3)) = 1.15
        
        _TimeOffset ("Time Offset", Range(0, 20)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay+150"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Stencil
            {
                Ref 0
                Comp Always
                Pass Keep
            }

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
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _DistortStrength;
            float _DistortSpeed;
            float _DistortScale;
            float _FlowSpeed;
            float _Turbulence;
            float _Softness;
            float _FadeAmount;
            float _Intensity;
            float _TimeOffset;

            float2 hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f*f*(3.0-2.0*f);

                float a = dot(hash22(i), f);
                float b = dot(hash22(i + float2(1.0,0.0)), f - float2(1.0,0.0));
                float c = dot(hash22(i + float2(0.0,1.0)), f - float2(0.0,1.0));
                float d = dot(hash22(i + float2(1.0,1.0)), f - float2(1.0,1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y + _TimeOffset;
                float2 uv = i.uv;

                // Multi-layer distortion
                float2 noiseUV = uv * _DistortScale;
                float2 d1 = float2(
                    noise(noiseUV + time * _DistortSpeed * 0.7),
                    noise(noiseUV * 1.35 + float2(13.57, 57.89) + time * _DistortSpeed * 1.15)
                );
                float2 d2 = float2(
                    noise(noiseUV * 2.4 + time * _DistortSpeed * 1.45 + float2(34.12, 78.90)),
                    noise(noiseUV * 0.75 - time * _DistortSpeed * 0.85 + float2(21.34, 65.43))
                );

                d1.y += time * _FlowSpeed * 0.45;
                d2.y += time * _FlowSpeed * 0.75;

                float2 distortion = (d1 + d2 * _Turbulence) * _DistortStrength;
                float2 sampleUV = uv + distortion;

                // Sample texture
                fixed4 tex = tex2D(_MainTex, sampleUV);

                // === CIRCULAR RADIAL SOFTNESS + EDGE FADE ===
                float2 centeredUV = uv * 2.0 - 1.0;                    // -1 to 1
                float distFromCenter = length(centeredUV);             // circular distance
                float radialFade = 1.0 - smoothstep(0.6, 1.0, distFromCenter * _Softness);

                // Extra fade for out-of-bounds sampling
                float2 boundsFade = smoothstep(0.0, 0.12, sampleUV) * smoothstep(1.0, 0.88, sampleUV);
                float finalFade = radialFade * boundsFade.x * boundsFade.y;

                // Final alpha
                float alpha = tex.a * finalFade * (1.0 - _FadeAmount);
                
                fixed4 c = tex * i.color;
                c.rgb *= _Intensity;
                c.a = alpha;

                // Premultiply alpha
                c.rgb *= c.a;

                return c;
            }
            ENDCG
        }
    }
}