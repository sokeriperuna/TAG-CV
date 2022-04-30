Shader "Hidden/Effects/CRT"
{

    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        #define tex2D(idx, uv) SAMPLE_TEXTURE2D(idx, sampler##idx, uv)
        #define sampler2D(idx) TEXTURE2D_SAMPLER2D(idx, sampler##idx)
        #define v2f VaryingsDefault

        sampler2D(_MainTex);
        int _Height;
        float _LineDarkness;
        float _Warping;
        float _VignetteIntensity;
        float _VignetteOpacity;

        float OneMinus(float v) { return 1-v; }
        float2 OneMinus(float2 v) {return 1-v; }

        float2 WarpUV(float2 uv){
            float2 delta = uv - 0.5;
            float delta2 = dot(delta, delta);
            float delta4 = dot(delta2, delta2);
            float deltaOffset = delta4*_Warping;

            return uv+ delta*deltaOffset;
        }


        float Border (float2 uv){
            float radius = min(_Warping, 0.08);
            radius = max(min(min(abs(radius * 2.0), abs(1.0)), abs(1.0)), 1e-5);
            float2 abs_uv = abs(uv * 2.0 - 1.0) - float2(1.0, 1.0) + radius;
            float dist = length(max(float2(0.0,0.0), abs_uv)) / radius;
            float square = smoothstep(0.96, 1.0, dist);//smoothstep(0.96, 1.0, dist);
            return clamp(1.0 - square, 0.0, 1.0);//s
        }

        float Vignette(float2 uv){
            float2 newUV = uv*OneMinus(uv);

            float vignette = newUV.x * newUV.y * (100/_VignetteIntensity) + _VignetteOpacity;

            return saturate(vignette);

            //return pow(_VignetteIntensity*_VignetteOpacity, vignette);

            //return OneMinus(pow(_VignetteIntensity * _VignetteOpacity, vignette));
        }

        half4 frag(v2f i) : SV_TARGET
        {
            // Warp tex
            float2 warpedUV = WarpUV(i.texcoord);


            // Apply scanlines
            half3 mainTex = tex2D(_MainTex, warpedUV);
            int scanlineRow = floor(warpedUV.y * _Height) %2;
            mainTex *= OneMinus(_LineDarkness) + _LineDarkness*scanlineRow;

            // Apply black border and vignette to hide imperfections in warping
            mainTex *= Border(warpedUV);
            mainTex *= Vignette(warpedUV);

            return half4(mainTex, 1);
        }

    ENDHLSL

    SubShader
    {
        ZTest Always Cull Off ZWrite On 
        Pass
        {
            HLSLPROGRAM
            
                #pragma vertex VertDefault
                #pragma fragment frag

            ENDHLSL
        }

    }

}
