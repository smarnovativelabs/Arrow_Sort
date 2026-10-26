Shader "WoolGame/EntranceMist"
{
 Properties { _Color("Mist",Color)=(.88,1,.78,1) }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent" }
  Pass
  {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A { float4 p:POSITION;float2 uv:TEXCOORD0; };
   struct V { float4 p:SV_POSITION;float2 uv:TEXCOORD0; };
   CBUFFER_START(UnityPerMaterial)
   half4 _Color;
   CBUFFER_END
   V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
   half4 frag(V i):SV_Target { float d=length((i.uv-.5)*2);return half4(_Color.rgb,_Color.a*(1-smoothstep(.55,1,d))); }
   ENDHLSL
  }
 }
}
