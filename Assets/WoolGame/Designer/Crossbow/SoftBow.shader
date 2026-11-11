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
        _RailWhite("White center rail on base mesh", Range(0,1)) = 0
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
                half _ShadeStrength, _Highlight, _Rim, _RailWhite;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; float2 palette : TEXCOORD2; float2 railPosition : TEXCOORD3; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 normalWS : TEXCOORD1; float3 positionWS : TEXCOORD2; float row : TEXCOORD3; float3 localPosition : TEXCOORD4; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.row = v.palette.x;o.localPosition=float3(v.railPosition.x,0,v.railPosition.y);
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
                half3 neutralCenter=lerp(half3(.84h,.82h,.72h),half3(1,.98h,.9h),saturate(diffuse*.8h+spec));
                                half3 result=lerp(neutralCenter,matched*shade,colored);
                half rail=(1-smoothstep(.158h,.172h,abs(i.localPosition.x)))*smoothstep(-.72h,-.69h,i.localPosition.z)*(1-smoothstep(.68h,.72h,i.localPosition.z));
                result=lerp(result,result*.78h,_RailWhite);
                                // Recessed neutral channel, bright side rails, and pale lower stop.
                half x=abs(i.localPosition.x);
                half channel=1-smoothstep(.139h,.151h,x);
                half edgeShadow=smoothstep(.10h,.14h,x);
                half3 channelColor=lerp(half3(.58h,.55h,.47h),half3(.36h,.34h,.29h),edgeShadow*.65h);
                half3 railColor=lerp(half3(.74h,.76h,.73h),half3(.98h,.99h,.95h),saturate(diffuse*.8h+.2h));
                half3 centerArea=lerp(railColor,channelColor,channel);
                half bottomStop=1-smoothstep(-.69h,-.63h,i.localPosition.z);
                centerArea=lerp(centerArea,half3(.83h,.85h,.81h),bottomStop);
                result=lerp(result,centerArea,rail*_RailWhite);
                return half4(result,1);
            }
            ENDHLSL
        }
    }
}
