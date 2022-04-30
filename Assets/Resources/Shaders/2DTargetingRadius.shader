Shader "TAG/UI/2DTargetingRadius"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FieldOfView ("Field of View", Range(0, 360)) = 360
        _MinRadius ("Minimum radius", Range(0, 1)) = 0
        _MaxRadius ("Maximum radius", Range(0, 1)) =1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
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
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _FieldOfView;
            float _MinRadius;
            float _MaxRadius;

            const float radius = 0.7071067f; // 1/sqrt(2) 


            float sqrMag(float2 v){ return dot(v, v); }

            float2 getUVCoordFromCenter(float2 coord) { return coord - float2(0.5f, 0.5f); }

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 fromCenter = getUVCoordFromCenter(i.uv);

                // Determine Angle
                float cosFoV = cos(radians(0.5f*_FieldOfView));
                float cosUV = fromCenter.x/length(fromCenter);
                float isInsideAngle = saturate(sign(cosUV - cosFoV));

                //Determine if is inside radius
                float minSqr = _MinRadius*_MinRadius;
                float sqrMagFromCenter = sqrMag(fromCenter);

                float isInsideRadius  = saturate(sign(sqrMagFromCenter - minSqr));
                isInsideRadius *= saturate(sign(0.25f- sqrMagFromCenter));

                float toBeRendered = isInsideRadius * isInsideAngle;

                fixed4 texSample = tex2D(_MainTex, i.uv);

                return texSample * toBeRendered*i.color *saturate(sqrMagFromCenter*3-0.1);
            }
            ENDCG
        }
    }
}
