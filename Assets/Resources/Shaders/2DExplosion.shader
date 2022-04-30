Shader "Unlit/2DExplosion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutsidePatternTex ("Outside Pattern", 2D) = "White" {}
        _InsidePatternTex ("Inside Pattern", 2D) = "White" {}
        _InsidePatternScrollingData("Inside Scrolling Data", 2D) = "Gray" {}
        _InsideScrollingSpeed("Inside Scrolling Speed", float) = 1
        _InnerCirleRadius("Inner Circle Radius", Range(0, 1)) = 0.5
    }
        SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
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
            sampler2D _OutsidePatternTex;
            sampler2D _InsidePatternTex;
            sampler2D _InsidePatternScrollingData;

            float4 _MainTex_ST;
            float4 _OutsidePatternTex_ST;
            float4 _InsidePatternTex_ST;

            float _InnerCirleRadius;
            float _InsideScrollingSpeed;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed OneMinus(fixed v) {return 1-v;}
            float OneMinus(float v) {return 1-v;}

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the textures
                fixed2 uvFromCenter = i.uv - fixed2(0.5, 0.5);

                fixed4 baseColor = tex2D(_MainTex, i.uv);

                fixed sqrMagFromCenter = dot(uvFromCenter, uvFromCenter);

                fixed insideOuterCircle = saturate(sign(0.25-sqrMagFromCenter));

                fixed innerCircleSqrMag = pow(_InnerCirleRadius/2, 2);
                fixed insideInnerCircle = saturate(sign(innerCircleSqrMag-sqrMagFromCenter));

                fixed4 outsideTexColor = tex2D(_OutsidePatternTex, TRANSFORM_TEX(i.uv, _OutsidePatternTex));

                fixed2 firstUV = i.uv;
                firstUV.y += _Time.y*0.5;
                fixed scrollingDir = tex2D(_InsidePatternScrollingData, firstUV).x-0.5;

                fixed2 insideUV =  firstUV;

                insideUV.x += _Time*scrollingDir*_InsideScrollingSpeed;

                fixed2 newUV = TRANSFORM_TEX(insideUV, _InsidePatternTex);
                fixed4 insideTexColor = tex2D(_InsidePatternTex, newUV);

                fixed4 composite = fixed4(0,0,0,0);
                composite += outsideTexColor * OneMinus(insideInnerCircle) * insideOuterCircle;
                composite += insideTexColor * insideInnerCircle;

                return composite*i.color;
            }
            ENDCG
        }
    }
}
