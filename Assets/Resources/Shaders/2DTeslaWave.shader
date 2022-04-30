Shader "TAG/UI/2DTeslaWave"
{
    Properties
    {
        _MainTex  ("Texture", 2D) = "white" {}
        _Noise  ("Noise", 2D) = "white" {}
        _Amplitude("Wave Amplitude", Range(0, 1)) = 1
        _Thickness("Wave Thickness", Range(0, 1)) = 0.2
        _BaseFreq("Baseline Wave Frequency", Range(0, 2)) = 1
        _SecondFreq("Secondary Wave Frequency", Range(0, 1)) = 0.5
        _NoiseFreq("Noise Texture Frequency", Range(0,20)) = 20
        _NoiseDamp("Noise Dampening", Range(0,1)) = 0
        _TimeOffset("Time Offset", Float) = 0
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
            float4 _MainTex_ST;

            sampler2D _Noise;
            float4 _Noise_ST;

            fixed _Amplitude;
            fixed _Thickness;
            fixed _BaseFreq;
            fixed _SecondFreq;
            fixed _NoiseFreq;
            fixed _NoiseDamp;
            fixed _TimeOffset;

            fixed Thicken(fixed yUV, fixed yValue, fixed thickness)
            {
                fixed tooHigh = sign(yUV-(yValue + thickness/2));
                fixed tooLow  = sign((yValue - thickness/2)-yUV);

                return (1-saturate((1+tooHigh)+(1+tooLow)));
            }
            fixed TriangleWave(fixed phase, fixed period)
            {
                return 2*abs(phase/period-floor(phase/period+0.5));
            }

            fixed InsideTriangleWave(fixed2 uvCoords, fixed phase, fixed period, fixed amplitude, fixed thickness)
            {
                fixed halfThickness = thickness/2;

                fixed yValue = TriangleWave(uvCoords.x+phase, period)*(1-thickness)+halfThickness;
                yValue *= amplitude;
                yValue += (1-amplitude)/2;

                return Thicken(0, yValue, halfThickness); // Obsolete
            }

            fixed AdjustForThickness(fixed yCoord, fixed thickness)
            {
                return yCoord*(1-thickness)+thickness/2;
            }

            fixed ApplyAmplitude(fixed yCoord, fixed amplitude)
            {
                return yCoord*amplitude+(1-amplitude)/2;
            }

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
                fixed4 time = _Time.y + _TimeOffset; 
                fixed4 noise = tex2D(_Noise, i.uv+time*_NoiseFreq)*0.8;
                fixed noiseSqrMag = dot(noise, noise);
                noiseSqrMag *= (1-_NoiseDamp);

                fixed thickness = _Thickness;
                thickness*= noiseSqrMag;

                //i.uv += noise*0.1;
                //i.uv = saturate(i.uv);

                fixed baselineWave = TriangleWave(i.uv.x+time*_BaseFreq, 0.5)/2;
                baselineWave += TriangleWave(i.uv.x-time, 0.5)/2;
                baselineWave -= TriangleWave(i.uv.x+time*_SecondFreq, 0.125)*0.2;
                //baselineWave += TriangleWave(i.uv.x+time*0.7, 0.25)*0.1;


                baselineWave = AdjustForThickness(baselineWave, thickness);
                baselineWave = ApplyAmplitude(baselineWave, _Amplitude);

                return i.color*Thicken(i.uv.y, baselineWave, thickness);


                //return fixed4(1,1,1,1)*InsideTriangleWave(i.uv, _Time.z, 0.5, _Amplitude, _Thickness);
            }
            ENDCG
        }
    }
}
