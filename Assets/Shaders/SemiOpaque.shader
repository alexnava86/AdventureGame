// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/SemiOpaque" 
{
    Properties
    {
        [PerRendererData] _MainTex ( "Sprite Texture", 2D ) = "white" {}
        _Color ( "Tint", Color ) = ( 1, 1, 1, 1 )
        [MaterialToggle] PixelSnap ( "Pixel snap", Float ) = 0
        _OccludedColor ( "Occluded Tint", Color ) = ( 0, 0, 0, 0.5 )
        _Cutoff("Cut off", float) = 0.1
    }

	CGINCLUDE
	// shared structs and vert program used in both the vert and frag programs
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
	sampler2D _MainTex;
    float _Cutoff;
	ENDCG

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

		Cull Off
		Blend One OneMinusSrcAlpha
		
        Pass
        {
            Stencil
            {
                Ref 1 //Ref 1 
                Comp Always//Always //if Greater, blends OccludedColor w/  Sprites w/ stencil layer 1
            }

        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

			v2f vert( appdata_t IN )  
			{
			    v2f OUT;
			    OUT.vertex = UnityObjectToClipPos( IN.vertex );
			    OUT.texcoord = IN.texcoord;
			    OUT.color = IN.color * _Color;
			    #ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

			    return OUT;
			}
			
            fixed4 frag( v2f IN ) : SV_Target
            {
                fixed4 c = tex2D( _MainTex, IN.texcoord ) * IN.color;
                c.rgb *= c.a;
                if(c.a < _Cutoff) discard;
                return c;
            }
        ENDCG
        }

        //Occluded pixel pass. Anything rendered here is behind an occluder
        Pass
        {
            Stencil
            {
                Ref 1
                Comp equal
            }

        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            fixed4 _OccludedColor;

			v2f vert( appdata_t IN )  
			{
			    v2f OUT;
			    OUT.vertex = UnityObjectToClipPos( IN.vertex );
			    OUT.texcoord = IN.texcoord;
			    OUT.color = IN.color * _Color;
			    #ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

			    return OUT;
			}

            fixed4 frag( v2f IN ) : SV_Target
            {
                fixed4 c = tex2D( _MainTex, IN.texcoord ) * IN.color;
                if(c.a < _Cutoff) discard;
                return _OccludedColor * c.a;
            }
        ENDCG
        }
        
    }
}
