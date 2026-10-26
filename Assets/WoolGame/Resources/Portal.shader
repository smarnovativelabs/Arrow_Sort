Shader "WoolGame/Portal"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION;float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0; };
            V vert(A v){V o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;return o;}
            half4 frag(V v):SV_Target
            {
                float2 p=(v.uv-.5)*2;float r=length(p),a=atan2(p.y,p.x);
                float spiral=.5+.5*sin(a*5-r*24+_Time.y*2);
                float3 color=lerp(float3(.05,.015,.14),float3(.36,.13,.68),spiral*(.35+.65*r));
                float rim=exp(-pow((r-.78)*23,2));
                color=lerp(color,float3(1,.65,.17)*(.76+.24*sin(a*38+r*20)),rim);
                color+=float3(.24,.08,.42)*exp(-pow((r-.80)*9,2));
                return half4(color,(1-smoothstep(.81,.99,r))*.97);
            }
            ENDHLSL
        }
    }
}
