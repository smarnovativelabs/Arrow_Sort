Shader "WoolGame/Shared Snake Gloss"
{
    Properties
    {
        _BaseMap("Original detail atlas", 2D) = "white" {}
        _TailRamp("Shared ten dark colors", 2D) = "white" {}
        _Shine("Surface shine", Range(0,1)) = 0.85
        _NeutralBrightness("Metal and markings", Range(0,1)) = 1
        _DetailStrength("Use original shading detail", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "SharedGloss"
            Tags { "LightMode"="UniversalForward" }
            Cull Back ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_TailRamp); SAMPLER(sampler_TailRamp);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Shine;
                float _NeutralBrightness;
                float _DetailStrength;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS:POSITION;
                float3 normalOS:NORMAL;
                float2 uv:TEXCOORD0;
                float2 palette:TEXCOORD2;
                float2 paint:TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS:SV_POSITION;
                float2 uv:TEXCOORD0;
                float3 normalWS:TEXCOORD1;
                float row:TEXCOORD2;
                float2 paint:TEXCOORD3;
                float3 viewWS:TEXCOORD4;
            };
            Varyings Vert(Attributes a)
            {
                UNITY_SETUP_INSTANCE_ID(a);
                Varyings o;
                float3 world=TransformObjectToWorld(a.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(world);
                o.normalWS=TransformObjectToWorldNormal(a.normalOS);
                o.viewWS=GetWorldSpaceViewDir(world);
                o.uv=TRANSFORM_TEX(a.uv,_BaseMap);
                o.row=a.palette.x;
                o.paint=a.paint;
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                half3 detail=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
                half high=max(detail.r,max(detail.g,detail.b));
                half low=min(detail.r,min(detail.g,detail.b));
                half saturation=(high-low)/max(high,.001h);
                half colored=smoothstep(.12h,.3h,saturation)*smoothstep(.015h,.06h,high);
                float3 n=SafeNormalize(i.normalWS);
                Light light=GetMainLight();
                half diffuse=saturate(dot(n,light.direction));
                float3 halfway=SafeNormalize(light.direction+SafeNormalize(i.viewWS));
                half reflection=pow(saturate(dot(n,halfway)),28);
                float2 spot=(i.paint-float2(.27,.73))*float2(4.8,6.0);
                half painted=exp2(-dot(spot,spot));
                half gloss=saturate(max(reflection*.65h,painted)*_Shine);
                half shade=.45h+.05h*diffuse;
                shade=lerp(shade,.99h,gloss);
                shade=lerp(shade,high,_DetailStrength);
                half3 color=SAMPLE_TEXTURE2D(_TailRamp,sampler_TailRamp,float2(shade,i.row)).rgb;
                half3 illumination=.75h+.25h*min(light.color,half3(1.2h,1.2h,1.2h))*diffuse;
                color*=lerp(illumination,half3(1,1,1),gloss);
                half3 neutral=detail*_NeutralBrightness;
                return half4(lerp(neutral,color,colored),1);
            }
            ENDHLSL
        }
    }
}
