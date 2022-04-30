Shader "Unlit/2DExplosionAlt3"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OuterCircleRadius("Outer Circle Radius", Range(0,1)) = 1
        _StripeCount("Stripes Count", float) = 1
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
            fixed4 _MainTex_ST;
            float _OuterCircleRadius;
            float _StripeCount;

            const fixed PI = 3.14159;

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

            fixed Triangle(fixed t, fixed period=1){
                return 2*abs(t/period-floor(t/period+0.5));
            }

            fixed Square(fixed t, fixed f=1){
                return 2*floor(f*t)-floor(2*f*t)+1;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                fixed outerRadius = _OuterCircleRadius*0.5;

                fixed2 uvFromCenter = i.uv - fixed2(0.5, 0.5);

                fixed cosUV = uvFromCenter.x/length(uvFromCenter);
                fixed angle = acos(cosUV);

                fixed4 baseColor = tex2D(_MainTex, i.uv);

                fixed sqrMagFromCenter = dot(uvFromCenter, uvFromCenter);

                fixed magFromCenter = sqrt(sqrMagFromCenter);
                magFromCenter -= Triangle(angle, 0.2)*0.02;

                fixed insideOuterCircle = saturate(sign(pow(outerRadius,2)-sqrMagFromCenter));
                //fixed insideOuterCircle = saturate(sign(pow(outerRadius,2)-sqrMagFromCenter));

                //fixed innerCircleSqrMag = pow(_InnerCirleRadius/2, 2);
                //fixed insideInnerCircle = saturate(sign(innerCircleSqrMag-sqrMagFromCenter));

                fixed4 composite = insideOuterCircle*Square(magFromCenter, _StripeCount);
                composite.w = 1;
                return composite*insideOuterCircle*i.color;
            }
            ENDCG
        }
    }
}