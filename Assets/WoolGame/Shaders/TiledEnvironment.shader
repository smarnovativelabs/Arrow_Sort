Shader "WoolGame/Tiled Environment"
{
    Properties
    {
        [MainTexture] _BaseMap("Environment image",2D)="white"{}
        [MainColor] _BaseColor("Tint",Color)=(1,1,1,1)
        _BoardCamera("Board camera",Float)=0
        _SlotCamera("Slot camera",Float)=0
        _TileMap("Reference floor source",2D)="white"{}
        _TileRect("Clean two-row reference patch",Vector)=(.046083,.847826,.082949,.119565)
        _TileScale("Viewport tile repeats",Vector)=(12,12,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float4 screen:TEXCOORD1; float2 ground:TEXCOORD2; };
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_TileMap); SAMPLER(sampler_TileMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST; half4 _BaseColor; float _BoardCamera; float _SlotCamera;
            float4 _TileRect; float4 _TileScale;
            CBUFFER_END
            Varyings Vert(Attributes v)
            {
                Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);
                o.screen=ComputeScreenPos(o.positionCS);o.uv=v.uv;o.ground=TransformObjectToWorld(v.positionOS.xyz).xz;return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                // The slot camera owns only lavender: never reveal a second tile row.
                if(_SlotCamera>.5)
                    return SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,float2(i.uv.x,clamp(i.uv.y,.554,.665)))*_BaseColor;
                if(_BoardCamera>.5||i.uv.y<.548)
                {
                    // One uninterrupted pattern in-game; planar continuation in the Scene view.
                    float2 repeats=_BoardCamera>.5?(i.screen.xy/i.screen.w)*_TileScale.xy:i.ground*float2(.6,.654545);
                    float2 patch=frac(repeats);
                    patch=clamp(patch,float2(.5/36,.5/33),1-float2(.5/36,.5/33));
                    return SAMPLE_TEXTURE2D(_TileMap,sampler_TileMap,_TileRect.xy+patch*_TileRect.zw);
                }
                return SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*_BaseColor;
            }
            ENDHLSL
        }
    }
}
