Shader "WoolGame/Reference Block Vehicle"
{
 Properties {
  _TailRamp("Shared snake and crossbow palette",2D)="white"{}
  _Shine("Restrained edge gloss",Range(0,1))=.5
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
   struct V {float4 p:SV_POSITION;float3 n:TEXCOORD0;float3 localNormal:TEXCOORD1;float3 view:TEXCOORD2;float row:TEXCOORD3;float2 paint:TEXCOORD4;float height:TEXCOORD5;};
   V Vert(A a){UNITY_SETUP_INSTANCE_ID(a);V o;float3 world=TransformObjectToWorld(a.p.xyz);o.p=TransformWorldToHClip(world);o.n=TransformObjectToWorldNormal(a.n);o.localNormal=a.n;o.view=GetWorldSpaceViewDir(world);o.row=a.row.x;o.paint=a.paint;o.height=a.p.y/1.475;return o;}
   half4 Frag(V i):SV_Target {
    float3 n=SafeNormalize(i.n);float3 localN=SafeNormalize(i.localNormal);Light light=GetMainLight();
    half top=saturate(localN.y);half diffuse=saturate(dot(n,light.direction));
    half edge=min(min(i.paint.x,1-i.paint.x),min(i.paint.y,1-i.paint.y));
    half rim=1-smoothstep(.045h,.12h,edge);
    half raisedRim=smoothstep(.95h,.985h,i.height);
    half roof=.40h+.10h*raisedRim+.025h*diffuse;
    half wall=.20h+.065h*diffuse;
    half shade=lerp(wall,roof,top);
    // A distinct shaded base and lit chamfer supply depth without changing hue rows.
    shade=lerp(.13h+.035h*diffuse,shade,smoothstep(.18h,.25h,i.height));
    half spec=pow(saturate(dot(n,SafeNormalize(light.direction+SafeNormalize(i.view)))),48);
    shade=saturate(shade+spec*.13h*_Shine);
    half3 color=SAMPLE_TEXTURE2D(_TailRamp,sampler_TailRamp,float2(shade,i.row)).rgb;
    // Dark inset roof and shaded inner rim reproduce the reference depth.
    half roofInset=top*(1-raisedRim)*smoothstep(.82h,.88h,i.height);
    half innerCrease=roofInset*(1-smoothstep(.12h,.22h,edge));
    color*=1-.18h*roofInset-.09h*innerCrease;
    color=lerp(color,half3(.88h,.95h,1),_SortWhite);
    return half4(color,1);
   }
   ENDHLSL
  }
 }
}
