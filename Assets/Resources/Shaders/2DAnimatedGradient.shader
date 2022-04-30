Shader "Unlit/2DAnimatedGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GradTex ("ColorGradient", 2D) = "white" {}
        _NoiseTex1 ("Noise Texture 1", 2D) = "white" {} 
        _NoiseTex2 ("Noise Texture 2", 2D) = "white" {} 
        _AnimSpeed ("Animation Speed", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _GradTex;
            sampler2D _NoiseTex1;
            sampler2D _NoiseTex2;

            fixed _AnimSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                // sample the texture


                float animSpeed = _AnimSpeed;
                fixed timeDisplacement = _Time.y*animSpeed;

                fixed4 spriteCol = tex2D(_MainTex, i.uv);
                fixed4 noise1 = tex2D(_NoiseTex1, i.uv+timeDisplacement*fixed2(-1,-1));
                fixed4 noise2 = tex2D(_NoiseTex2, i.uv+timeDisplacement);

                fixed noiseInterpolation = lerp(noise1, noise2, (0.5+0.5*sin(_Time.y* animSpeed)));

                fixed4 mainColor = tex2D(_MainTex, i.uv);

                fixed4 grad = tex2D(_GradTex, frac(pow((noise1+noise2), 2)));

                fixed4 composite = spriteCol * grad;
                composite.w = spriteCol.w;

                return composite;
            }
            ENDCG
        }
    }
}
