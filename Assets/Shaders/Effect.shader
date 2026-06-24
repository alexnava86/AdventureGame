Shader "Custom/Effect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.02
        _Intensity ("Glow Intensity", Range(0, 5)) = 1.8
        _Brightness ("Brightness", Range(0, 2)) = 1.1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay+100"           // Very high priority
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always                          // Ignore depth completely
        Blend One One

        Pass
        {
            // Force disable stencil entirely
            Stencil
            {
                Ref 0
                Comp Always
                Pass Keep
                Fail Keep
                ZFail Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
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

            sampler2D _MainTex;
            fixed4 _Color;
            float _Cutoff;
            float _Intensity;
            float _Brightness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                c.rgb *= _Brightness * _Intensity;
                
                clip(c.a - _Cutoff);
                
                c.rgb *= c.a;        // Premultiplied alpha (matches your other shaders)
                
                return c;
            }
            ENDCG
        }
    }
}