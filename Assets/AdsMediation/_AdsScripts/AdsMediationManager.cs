using System;
using System.Collections;
using UnityEngine;
namespace AdsMediation
{
    [RequireComponent(typeof(GoogleMobileAdsConsentController),typeof(AdMobController))]
    public sealed class AdsMediationManager : MonoBehaviour
    {
        public static AdsMediationManager instance;
        public bool isAdsRemoved,displayAdmobBanner,sessionFirstInterstitial=true;
        public float interAdDelayTimer=100;
        public float InterstitialInterval=100;
        public delegate void RewardedVieoResponse(string response);
        AdMobController ads;
        GoogleMobileAdsConsentController consent;
        bool initializing,initializationFinished,rewardBusy;
        public event Action<bool> AdVisibilityChanged;
        void Awake()
        {
            if(instance&&instance!=this){Destroy(gameObject);return;}
            instance=this;DontDestroyOnLoad(gameObject);
            ads=GetComponent<AdMobController>();consent=GetComponent<GoogleMobileAdsConsentController>();
            // Google's demo units: this project's current configuration is test-only.
            ads.androidBannerId="";
            ads.androidInterstitialId="";
            ads.androidRewardedVideoId="";
            ads.iosBannerId="";
            ads.iosInterstitialId="";
            ads.iosRewardedVideoId="";
        }
        void Start(){InitializeMediationAds(1);}
        public void InitializeMediationAds(int mediation=1)
        {
            if(initializing||ads.isInitialized)return;
            initializing=true;initializationFinished=false;
            StartCoroutine(Initialize());
        }
        IEnumerator Initialize()
        {
#if UNITY_EDITOR
            ads.InitializeAds(true,false);
#elif UNITY_ANDROID || UNITY_IOS
            bool returned=false;
            consent.GatherConsent(error=>{returned=true;if(!string.IsNullOrEmpty(error))Debug.LogWarning("Ads consent: "+error);});
            float deadline=Time.realtimeSinceStartup+30;
            while(!returned&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!consent.CanRequestAds){initializing=false;initializationFinished=true;yield break;}
            ads.InitializeAds(true,false);
#else
            initializing=false;initializationFinished=true;yield break;
#endif
            float timeout=Time.realtimeSinceStartup+20;
            while(!ads.isInitialized&&Time.realtimeSinceStartup<timeout)yield return null;
            initializing=false;initializationFinished=true;
        }
        public void CheckForConsent(){InitializeMediationAds(1);}
        public bool IsAdsInitialized()=>ads&&ads.isInitialized;
        public bool GetInitializationResponse()=>initializationFinished;
        public bool CanShowInterstitial()=>!isAdsRemoved&&!WoolGame.WoolProgress.NoAdsActive&&!rewardBusy&&interAdDelayTimer<=0&&ads.IsInterstitialLoaded();
        public void ShowInterstitial(){if(!CanShowInterstitial())return;interAdDelayTimer=InterstitialInterval;AdVisibilityChanged?.Invoke(true);try{ads.ShowInterstitialAd();}catch(Exception error){Debug.LogException(error);OnInterstitialClosed();}}
        public void ShowTestInterstitial(){ShowInterstitial();}
        public void ShowRewardedVideo(RewardedVieoResponse success=null,RewardedVieoResponse failure=null)
        {
            if(rewardBusy){failure?.Invoke("An ad is already open.");return;}
            if(!IsAdsInitialized()||!ads.CanShowRewaredVideo())
            {InitializeMediationAds(1);failure?.Invoke("The video is not ready yet. Please try again shortly.");return;}
            rewardBusy=true;AdVisibilityChanged?.Invoke(true);
            bool finished=false;
            Action<bool,string> complete=(earned,message)=>
            {
                if(finished)return;finished=true;rewardBusy=false;AdVisibilityChanged?.Invoke(false);
                interAdDelayTimer=InterstitialInterval;
                if(earned)success?.Invoke(message);else failure?.Invoke(message);
            };
            try { ads.ShowRewardedAd(message=>complete(true,message),message=>complete(false,message)); }
            catch(Exception error){Debug.LogException(error);complete(false,"The video could not open. Please try again.");}
        }
        public void OnRVWatchSuccess(){interAdDelayTimer=InterstitialInterval;FirebaseManager.instance?.CallFirebasEvent("rewarded_ad_complete");}
        public void OnInterstitialClosed(){interAdDelayTimer=InterstitialInterval;AdVisibilityChanged?.Invoke(false);}
        public void OnRemoveAds(){isAdsRemoved=true;RemoveBannerAd();}
        public void RemoveBannerAd(){if(ads)ads.RemoveBanner();}
        public void DisplayBanner(){RemoveBannerAd();} // This game has no banner placement.
        public void DisplayApplovinBannerAd(){DisplayBanner();}
        public void HideApplovinBannarAd(){RemoveBannerAd();}
        void Update(){interAdDelayTimer=Mathf.Max(0,interAdDelayTimer-Time.unscaledDeltaTime);}
        void OnDestroy(){if(instance==this)instance=null;}
    }
}
