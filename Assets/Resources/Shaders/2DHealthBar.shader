Shader "TAG/UI/2DHealthBar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Color Mask", 2D) = "white" {}
        _FillColor ("HP Fill Color", Color) = (1, 1, 1, 1)
        _HPProg ("HP Progress", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Opaque" }
        LOD 100
        Cull off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;

            fixed4 _FillColor;

            fixed _HPProg;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the textures
                fixed4 baseColor = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.uv);


                fixed maskValue = saturate((mask.x+mask.y+mask.z)) ;

                fixed isFill  = saturate(sign(0.01+_HPProg-i.uv.x));
                fixed notFill = saturate(sign(i.uv.x-_HPProg));


                float4 fill = i.color * maskValue * isFill;


                /// WORKING!
                //fixed4 compositeColor = baseColor * _FillColor * isFill + baseColor*notFill - notFill*maskValue; 
                /// Alt
                fixed4 compositeColor = fill + (i.color-0.25)*baseColor.w*(1-maskValue);


                return compositeColor;
            }
            ENDCG
        }
    }
}
