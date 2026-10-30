using UnityEngine;
namespace WoolGame
{
    public sealed class WoolServices : MonoBehaviour
    {
        public static AdsMediation.AdsMediationManager EnsureAds()
        {
            if(AdsMediation.AdsMediationManager.instance)return AdsMediation.AdsMediationManager.instance;
            var go=new GameObject("Wool services - AppLovin MAX");
            return go.AddComponent<AdsMediation.AdsMediationManager>();
        }
    }
}
