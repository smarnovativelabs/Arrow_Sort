
Shader "WoolGame/SoftFog"
{
    Properties
    {
        _Color ("Fog Color", Color) = (1, 1, 1, 1)

        _Density ("Maximum Density", Range(0, 1)) = 0.9

        _StartFade ("Fade Start", Range(0, 1)) = 0.5

        _EndFade ("Fade End", Range(0, 1)) = 0.0

        _MinimumDensity ("Bottom Density", Range(0, 1)) = 0.0
        _SideFade ("Side Fade", Range(0, 0.5)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "Soft Fog"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Density;
                half _StartFade;
                half _EndFade;
                half _MinimumDensity;
                half _SideFade;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half y = input.uv.y;

                // Full fog above the fade start.
                // Smooth transition below it.
                half fade = smoothstep(
                    _EndFade,
                    max(_EndFade + 0.001h, _StartFade),
                    y
                );

                half density = lerp(
                    _MinimumDensity,
                    _Density,
                    fade
                );

                return half4(
                    _Color.rgb,
                    saturate(_Color.a * density) * (_SideFade > 0 ? smoothstep(0, _SideFade, min(input.uv.x, 1-input.uv.x)) : 1)
                );
            }

            ENDHLSL
        }
    }
}
