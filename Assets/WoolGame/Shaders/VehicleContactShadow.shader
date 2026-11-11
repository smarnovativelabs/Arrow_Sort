Shader "WoolGame/Vehicle Contact Shadow"{SubShader{Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent"}Pass{Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
HLSLPROGRAM
#pragma multi_compile_instancing
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};V vert(A a){UNITY_SETUP_INSTANCE_ID(a);V o;o.p=TransformObjectToHClip(a.p.xyz);o.uv=a.uv*2-1;return o;}half4 frag(V i):SV_Target{float d=dot(i.uv,i.uv);return half4(.055,.075,.065,.19*(1-smoothstep(.12,1,d)));}
ENDHLSL}}}
