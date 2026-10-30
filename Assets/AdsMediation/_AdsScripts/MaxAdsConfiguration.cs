using UnityEngine;
namespace AdsMediation
{
    [CreateAssetMenu(menuName="Arrow Sort/AppLovin MAX configuration")]
    public sealed class MaxAdsConfiguration : ScriptableObject
    {
        [Tooltip("From the AppLovin account Keys page.")] public string sdkKey="";
        public string androidInterstitialId="",androidRewardedId="";
        public string tenjinSdkKey="";
        public string androidAdMobInterstitialId="",androidAdMobRewardedId="";
        public string iosInterstitialId="",iosRewardedId="";
        // Retain the project's existing consent flow until its CMP setup is replaced.
        public string InterstitialId {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return iosInterstitialId;
#else
                return androidInterstitialId;
#endif
            }
        }
        public string RewardedId {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return iosRewardedId;
#else
                return androidRewardedId;
#endif
            }
        }
        #if UNITY_EDITOR
        void OnValidate()
        {
            if(string.IsNullOrWhiteSpace(sdkKey))return;
            var settings=UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/MaxSdk/Resources/AppLovinSettings.asset");
            if(!settings)return;
            var serialized=new UnityEditor.SerializedObject(settings);
            var key=serialized.FindProperty("sdkKey");
            if(key!=null&&key.stringValue!=sdkKey){key.stringValue=sdkKey;serialized.ApplyModifiedPropertiesWithoutUndo();UnityEditor.EditorUtility.SetDirty(settings);}
        }
#endif
        public bool IsConfigured=>!string.IsNullOrWhiteSpace(sdkKey)&&!string.IsNullOrWhiteSpace(InterstitialId)&&!string.IsNullOrWhiteSpace(RewardedId);
    }
}
