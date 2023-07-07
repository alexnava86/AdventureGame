// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/CharacterTEST"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_SwapTex("Color Data", 2D) = "transparent" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_BlinkColor ("Blink Color", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _Cutoff("Cut off", float) = 0.1
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
        AlphaTest Greater 0.5
        Blend One OneMinusSrcAlpha
            
        Stencil
            {
                Ref 1 //Ref 1
                Comp Always //Comp Always
                Pass replace //Pass replace
            }    
		Pass
		{
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
		
            fixed4 _Color;
            fixed4 _BlinkColor;
            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float _AlphaSplitEnabled;
            float _Cutoff;
            sampler2D _SwapTex;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c =  tex2D(_MainTex, IN.texcoord) * IN.color; //SampleSpriteTexture (IN.texcoord);
				fixed4 swapColor = tex2D(_SwapTex, float2(c.x, 0));
				c.rgb = lerp( c.rgb, _BlinkColor.rgb, _BlinkColor.a );

				fixed4 final = lerp(c, swapColor * _BlinkColor, swapColor.a) * IN.color;

				final.a = c.a;
				final.rgb *= c.a;
                if(c.a < _Cutoff) discard;
				return final;
			}
		ENDCG
		}
	}
}