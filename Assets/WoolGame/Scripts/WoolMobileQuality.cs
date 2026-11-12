using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WoolGame
{
    public enum WoolQualityTier { UltraLow, Low, Balanced, High }

    // Selected once behind the startup loading overlay; fixed for this app session.
    public static class WoolMobileQuality
    {
        public static WoolQualityTier CurrentTier { get; private set; } = WoolQualityTier.High;
        public static bool LowMemory => CurrentTier <= WoolQualityTier.Low;
        public static bool BalancedMemory => CurrentTier == WoolQualityTier.Balanced;
        public static bool MinimalEffects => CurrentTier == WoolQualityTier.UltraLow;
        static UniversalRenderPipelineAsset runtimePipeline;
        static bool initialized;
        static int highTargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            CurrentTier = WoolQualityTier.High;
            runtimePipeline = null;
            initialized = false;
            highTargetFrameRate = 60;
        }

        public static WoolQualityTier InitialTier(int ramMB, int gpuMemoryMB, int cpuCores)
        {
            var tier = ramMB <= 0 ? WoolQualityTier.Balanced : ramMB <= 1536 ? WoolQualityTier.UltraLow :
                ramMB <= 2560 ? WoolQualityTier.Low : ramMB <= 3584 ? WoolQualityTier.Balanced : WoolQualityTier.High;
            // Conservative hardware hints select the startup budget; gameplay never changes it.
            if (gpuMemoryMB > 0 && gpuMemoryMB <= 128) tier = WoolQualityTier.UltraLow;
            else if (gpuMemoryMB > 0 && gpuMemoryMB <= 256 && tier > WoolQualityTier.Low) tier = WoolQualityTier.Low;
            else if (gpuMemoryMB > 0 && gpuMemoryMB <= 512 && tier > WoolQualityTier.Balanced) tier = WoolQualityTier.Balanced;
            if (cpuCores > 0 && cpuCores <= 2 && tier > WoolQualityTier.Low) tier = WoolQualityTier.Low;
            return tier;
        }

        public static int FrameRateForTier(WoolQualityTier tier, float refreshHz)
        {
            if (tier != WoolQualityTier.High) return 30;
            if (refreshHz <= 0 || float.IsNaN(refreshHz) || float.IsInfinity(refreshHz)) return 60;
            int refresh = Mathf.Max(1, Mathf.RoundToInt(refreshHz));
            int divisor = Mathf.Max(1, Mathf.CeilToInt(refresh / 60f));
            return Mathf.Clamp(Mathf.CeilToInt((float)refresh / divisor), 1, 60);
        }

        public static int CurrentTargetFrameRate => CurrentTier == WoolQualityTier.High ? highTargetFrameRate : 30;

        public static void Apply()
        {
            if (!Application.isMobilePlatform || Application.isEditor || initialized) return;
            initialized = true;
            highTargetFrameRate = FrameRateForTier(WoolQualityTier.High, (float)Screen.currentResolution.refreshRateRatio.value);
            CurrentTier = InitialTier(SystemInfo.systemMemorySize, SystemInfo.graphicsMemorySize, SystemInfo.processorCount);
            // Only change texture uploads during startup; never cause synchronous
            // texture re-uploads while the player is moving vehicles.
            int mipLimit = CurrentTier == WoolQualityTier.UltraLow ? 2 : CurrentTier == WoolQualityTier.Low ? 1 : 0;
            QualitySettings.globalTextureMipmapLimit = Mathf.Max(QualitySettings.globalTextureMipmapLimit, mipLimit);
            ApplyTier();
        }

        static void ApplyTier()
        {
            Application.targetFrameRate = CurrentTargetFrameRate;
            if (CurrentTier == WoolQualityTier.High) return; // Keep authored full quality.
            if (!runtimePipeline)
            {
                var source = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                if (!source) return;
                runtimePipeline = Object.Instantiate(source);
                runtimePipeline.name = source.name + " (mobile runtime)";
                QualitySettings.renderPipeline = runtimePipeline;
            }
            float scale = CurrentTier == WoolQualityTier.UltraLow ? .55f : CurrentTier == WoolQualityTier.Low ? .75f : .85f;
            runtimePipeline.renderScale = Mathf.Min(runtimePipeline.renderScale, scale);
            runtimePipeline.supportsHDR = false;
            runtimePipeline.msaaSampleCount = 1;
        }

        public static Vector2Int TransitTextureSize(int width, int height) => TextureSize(CurrentTier, width, height);
        public static Vector2Int TextureSize(WoolQualityTier tier, int width, int height)
        {
            float scale = tier == WoolQualityTier.UltraLow ? Mathf.Min(.55f, 540f / Mathf.Max(1, width)) :
                tier == WoolQualityTier.Low ? Mathf.Min(.75f, 720f / Mathf.Max(1, width)) :
                tier == WoolQualityTier.Balanced ? Mathf.Min(.85f, 900f / Mathf.Max(1, width)) : 1f;
            return new Vector2Int(Mathf.Max(1, Mathf.RoundToInt(width * scale)), Mathf.Max(1, Mathf.RoundToInt(height * scale)));
        }
    }

}
