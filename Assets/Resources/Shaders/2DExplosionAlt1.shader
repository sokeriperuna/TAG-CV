Shader "Unlit/2DExplosionAlt1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _InsidePatternTex ("Inside Pattern", 2D) = "White" {}
        _InsidePatternDataTex("Inside Pattern Data", 2D) = "Gray" {}
        _InsideFlickerSpeed("Inside Flicker Speed", float) = 1
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
            sampler2D _InsidePatternTex;
            sampler2D _InsidePatternDataTex;


            float4 _MainTex_ST;
            float4 _InsidePatternTex_ST;
            float4 _InsidePatternDataTex_ST;

            float _InsideFlickerSpeed;
            float _InnerCirleRadius;


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

            fixed Triangle(fixed t, fixed period){
                return 2*abs(t/period-floor(t/period+0.5));
            }

            fixed Square(fixed t, fixed period){
                return round(frac(0.5+t/period));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 uvFromCenter = i.uv - fixed2(0.5, 0.5);

                fixed4 baseColor = tex2D(_MainTex, i.uv);

                fixed sqrMagFromCenter = dot(uvFromCenter, uvFromCenter);

                fixed insideOuterCircle = saturate(sign(0.25-sqrMagFromCenter));

                fixed innerCircleSqrMag = pow(_InnerCirleRadius/2, 2);
                fixed insideInnerCircle = saturate(sign(innerCircleSqrMag-sqrMagFromCenter));



                fixed4 outsideTexColor = (1,1,1,1);

                fixed2 insidePatternUV = TRANSFORM_TEX(i.uv, _InsidePatternTex);
                fixed4 insidePatternColor = tex2D(_InsidePatternTex, insidePatternUV);

                fixed2 insidePatternDataUV = TRANSFORM_TEX(i.uv, _InsidePatternDataTex);
                fixed4 insidePatternData = tex2D(_InsidePatternDataTex, insidePatternDataUV);

                fixed4 composite = fixed4(0,0,0,0);
                composite += outsideTexColor * OneMinus(insideInnerCircle) * insideOuterCircle;
                fixed t = Square(_Time.y*_InsideFlickerSpeed, 1);
                composite += insidePatternData * insidePatternColor * t * insideInnerCircle;
                composite += OneMinus(insidePatternData)*insidePatternColor*OneMinus(t)*insideInnerCircle;
                //composite += insidePatternData * insideInnerCircle * Triangle(_Time.y*_InsideFlickerSpeed, 1);
                //composite -= insidePatternColor * insideInnerCircle;

                composite.w = insideOuterCircle;
                return composite*i.color;
            }
            ENDCG
        }
    }
}
