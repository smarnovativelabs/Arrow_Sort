Shader "WoolGame/Soft Bow"
{
    Properties
    {
        [MainTexture] _BaseMap("Designer color atlas", 2D) = "white" {}
        _TailRamp("Shared snake color palette", 2D) = "white" {}
        [MainColor] _BaseColor("Tint", Color) = (1,1,1,1)
        _ShadeStrength("Soft shading", Range(0,1)) = 0.32
        _Highlight("Satin highlight", Range(0,1)) = 0.16
        _Rim("Edge light", Range(0,1)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Back
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_TailRamp); SAMPLER(sampler_TailRamp);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _ShadeStrength, _Highlight, _Rim;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; float2 palette : TEXCOORD2; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 normalWS : TEXCOORD1; float3 positionWS : TEXCOORD2; float row : TEXCOORD3; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.row = v.palette.x;
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                half3 n = normalize(i.normalWS);
                half3 view = GetWorldSpaceNormalizeViewDir(i.positionWS);
                // A stable broad key keeps the ten gameplay colors readable in every slot.
                half3 light = normalize(half3(-0.45, 0.8, -0.35));
                half diffuse = dot(n, light) * 0.5h + 0.5h;
                half shade = lerp(1.0h - _ShadeStrength, 1.04h, smoothstep(0.05h, 0.95h, diffuse));
                half spec = pow(saturate(dot(n, normalize(light + view))), 28.0h) * _Highlight;
                half rim = pow(1.0h - saturate(dot(n, view)), 3.0h) * _Rim;
                half3 base = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb;
                half high = max(base.r, max(base.g, base.b));
                half low = min(base.r, min(base.g, base.b));
                half colored = smoothstep(.12h, .3h, (high-low)/max(high,.001h)) * smoothstep(.015h,.06h,high);
                // UV3 stores the same color index used by vehicles; ramp rows come from the snake atlas.
                half rampShade = lerp(.45h + .05h * diffuse, .99h, saturate(spec + rim));
                half3 matched = SAMPLE_TEXTURE2D(_TailRamp, sampler_TailRamp, float2(rampShade, i.row)).rgb;
                return half4(lerp(base * shade + spec + rim, matched * shade, colored), 1);
            }
            ENDHLSL
        }
    }
}
