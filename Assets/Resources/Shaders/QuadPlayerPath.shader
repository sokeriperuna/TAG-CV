Shader "TAG/UI/QuadPlayerPath"
{
    Properties
    {
        _MainTex ("Pattern mask", 2D) = "PatternMask" {}
        _BaseColor ("Base color", Color) = (0,0,0,0)
        _ContrastColor ("Contrast color", Color) = (1,1,1,1)
        _AnimateSpeed ("Animation speed", float) = 1
        _PathLength("Path Length", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull off
        Blend One Zero
        //Blend SrcColor One
        //Blend SrcAlpha OneMinusSrcAlpha

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
                float2 magMap : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 magMap : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BaseColor;
            float4 _ContrastColor;
            float _AnimateSpeed;
            float _PathLength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.magMap = v.magMap;

                v.uv.x *= _PathLength;//o.magMap.x; // Set x-tiling to magnitude
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Transform the texture UVs
                float2 uv = float2(i.uv.x - frac(_Time.y * _AnimateSpeed), i.uv.y); //i.uv - frac(_Time.y * _AnimateSpeed); // We offset repatedly with frac to make it look like its looping

                // sample the texture
                float4 mask = tex2D(_MainTex, uv);

                float3 w = (1,1,1);
                float3 b = (0,0,0);

                float4 inverseMask = float4(clamp(w-mask.xyz, b, w), 1);

                float4 compositeColor = inverseMask * _BaseColor + mask * _ContrastColor;

                return compositeColor;
            }
            ENDCG
        }
    }
}