Shader "WoolGame/Raised Vehicle Gloss"
{
 Properties {
  _TailRamp("Original ten gameplay colors",2D)="white"{}
  _Shine("Soft gloss",Range(0,1))=.65
  _SortWhite("Sort reveal",Range(0,1))=0
 }
 SubShader {
  Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
  Pass {
   Tags {"LightMode"="UniversalForward"}
   Cull Back ZWrite On
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_TailRamp);SAMPLER(sampler_TailRamp);
   CBUFFER_START(UnityPerMaterial)
   float _Shine;float _SortWhite;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;float2 row:TEXCOORD2;float2 paint:TEXCOORD3;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V {float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 localNormal:TEXCOORD1;float3 view:TEXCOORD2;float row:TEXCOORD3;float2 paint:TEXCOORD4;};
   V Vert(A a){UNITY_SETUP_INSTANCE_ID(a);V o;float3 world=TransformObjectToWorld(a.p.xyz);o.p=TransformWorldToHClip(world);o.n=TransformObjectToWorldNormal(a.n);o.localNormal=a.n;o.view=GetWorldSpaceViewDir(world);o.row=a.row.x;o.paint=a.paint;return o;}
   half4 Frag(V i):SV_Target {
    float3 n=SafeNormalize(i.n);float3 localN=SafeNormalize(i.localNormal);Light light=GetMainLight();
    half top=saturate(localN.y);half diffuse=saturate(dot(n,light.direction));
    half shade=.27h+.33h*top+.065h*diffuse;
    half spec=pow(saturate(dot(n,SafeNormalize(light.direction+SafeNormalize(i.view)))),40);
    float2 spot=(i.paint-float2(.25,.77))*float2(5.8,7.2);
    half softSpot=exp2(-dot(spot,spot))*.42h*top;
    shade=lerp(shade,.91h,saturate(max(spec*.7h,softSpot)*_Shine));
    half3 color=SAMPLE_TEXTURE2D(_TailRamp,sampler_TailRamp,float2(shade,i.row)).rgb;
    // A soft roof highlight and small clearcoat reflection retain each palette hue.
    half3 highlight=SAMPLE_TEXTURE2D(_TailRamp,sampler_TailRamp,float2(.98h,i.row)).rgb;
    color=lerp(color,highlight,saturate((softSpot*.65h+spec*.24h)*_Shine));
    color=lerp(color,half3(.88h,.95h,1),_SortWhite);
    return half4(color,1);
   }
   ENDHLSL
  }
 }
}
