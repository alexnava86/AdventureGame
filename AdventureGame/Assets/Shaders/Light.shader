// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Light" 
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _OccludedColor ( "Occluded Tint", Color ) = ( 0, 0, 0, 0.5 )
        _Cutoff("Cut off", float) = 0.1
    }

 CGINCLUDE
 // shared structs and vert program used in both the vert and frag programs
 struct appdata_t //in
 {
     float4 vertex   : POSITION;
     float4 color    : COLOR;
     float2 texcoord : TEXCOORD0;
 };

 struct v2f //out
 {
     float4 vertex   : SV_POSITION;
     fixed4 color    : COLOR;
     half2 texcoord  : TEXCOORD0;
 };
         
 sampler2D _MainTex;
 float _Cutoff;
 fixed4 _Color;
 ENDCG

    SubShader
    {
     Tags
     { 
         "Queue"="Transparent-1" 
         "IgnoreProjector"="True" 
         "RenderType"="Transparent" 
         "PreviewType"="Plane"
         "CanUseSpriteAtlas"="True"
     }

     Cull Off
     AlphaTest Greater 0
     Blend DstColor SrcAlpha //DstColor//One//OneMinusSrcAlpha
     
        Pass
        {
            Stencil
            {
             Ref 2 //Ref 2//Ref 0
             Comp NotEqual //Comp Greater// //Comp Always// Comp NotEqual//Comp Less//Lequal
             Pass Replace //Pass replace
            }

        CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         #pragma multi_compile _ PIXELSNAP_ON
         #include "UnityCG.cginc"

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

         fixed4 frag(v2f IN) : SV_Target
         {
             fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
             if(c.a < _Cutoff) discard;
             c.rgb *= c.a; 
             return c;
         }
        ENDCG
        }
        /*
        Use stencil to tell wether or not a light has already been drawn there. 
        If so, doesnt draw the light again. This way you prevent overlapping shadows and added alphas
        */
        //Occluded pixel pass. Anything rendered here is behind an occluder

        Pass
        {
            Stencil
            {
                Ref 2
                Comp Equal
                Pass Replace
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
                 fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                 c.rgb *= c.a;
                 if(c.a < _Cutoff) discard;
                 return (_OccludedColor+c.a)/2; //(_OccludedColor+c.a)/2;
            }
        ENDCG
        }
    }
}