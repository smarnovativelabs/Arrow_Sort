using UnityEngine.Events;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System.Collections.Generic;

namespace AdsMediation
{
    public class AdMobController : MonoBehaviour
    {
        [Header("Android Ids")]
        public string androidBannerId;
        public string androidInterstitialId;
        public string androidRewardedVideoId;

        [Header("Ios Ids")]
        public string iosBannerId;
        public string iosInterstitialId;
        public string iosRewardedVideoId;

        public bool isInitialized = false;

        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;

        bool isPersonalizedads;
        bool rewardedInterReceived = false;
        bool rvRewardReceived = false;
        bool initiallyAdsLoaded;
        bool loadBannerMust;
        bool loadAdsOnInitialization;

        int interRetryAttempt;
        int rvRetryAttempt;
        int rvInterRetryAttempy;

        float interstitialLoadTime;
        AdsMediationManager.RewardedVieoResponse rvSuccessEvent, rvFailEvent;

        #region UNITY MONOBEHAVIOR METHODS

        public void InitializeAds(bool _loadAds=true,bool _loadBanner=false)
        {
            print("Initializing Admob Ads -" + isInitialized);

            if (isInitialized)
            {
                if (_loadAds && !initiallyAdsLoaded)
                {
                    initiallyAdsLoaded = true;
                    RequestAndLoadRewardedAd();
                    RequestAndLoadInterstitialAd();

                }
                if (_loadBanner)
                    RequestBannerAd();
                return;
            }
            Debug.Log("Initializing Admob Ads");
            loadAdsOnInitialization = _loadAds;
            loadBannerMust = _loadBanner;
            //RequestConfiguration _config = new RequestConfiguration.Builder.SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified).build();
            //MobileAds.SetRequestConfiguration(_config);
            // AdColonyAppOptions.SetGDPRRequired(true);
            //   AdColonyAppOptions.SetGDPRConsentString(AdColonyAppOptions.GetGDPRConsentString());
            //AppLovin.Initialize();
#if UNITY_IPHONE
              MobileAds.SetiOSAppPauseOnBackground(true);

            //if (AudiocallManager.PersonalizedAdStatus==0)
            //{
            //    //Show Personalized Ads
            //    isPersonalizedads = true;
            //}
            //else
            //{
            //    //Show NonPersonalized Ads
            //    isPersonalizedads = false;
            //}
#endif
            MobileAds.RaiseAdEventsOnUnityMainThread = true;   // copy from other script.. by write this. every events we receive from admob will run on unity main thread
            MobileAds.Initialize(HandleInitCompleteAction);

        }
        private void HandleInitCompleteAction(InitializationStatus initstatus)
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {

                Debug.Log("AdMob Initialization Complete");

                var adapterStatusMap = initstatus.getAdapterStatusMap();
                foreach (var adapter in adapterStatusMap)
                {
                    Debug.Log($"Adapter: {adapter.Key}, Status: {adapter.Value.InitializationState}");
                    if (adapter.Value.InitializationState == AdapterState.NotReady)
                    {
                        Debug.LogWarning($"Adapter {adapter.Key} failed to initialize: {adapter.Value.Description}");
                    }
                }
                isInitialized = true;
             //   FirebaseManager.instance?.CallFirebasEvent("AdmobInitialized");
                if (loadBannerMust)
                {
                    RequestBannerAd();
                }
                initiallyAdsLoaded = true;
                RequestAndLoadRewardedAd();
                RequestAndLoadInterstitialAd();

                //if (loadAdsOnInitialization)
                //{
                    

                //}
            });
        }

        private void Update()
        {
            if (isInitialized)
            {
                if (interstitialLoadTime > 0)
                {
                    interstitialLoadTime -= Time.deltaTime;
                    if (interstitialLoadTime <= 0)
                    {
                        RequestAndLoadInterstitialAd();
                    }
                }
            }
        }
        #endregion


        #region BANNER ADS
        public void RemoveBanner()
        {
            if (!isInitialized)
                return;
            DestroyBannerAd();
        }
        public void DisplayBanner()
        {
            if (!isInitialized || !loadBannerMust)
                return;
            RequestBannerAd();
        }
        // call this function to load the banner Ad
        public void RequestBannerAd()
        {

            // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
                    string adUnitId = androidBannerId;
#elif UNITY_IPHONE
                    string adUnitId = iosBannerId;
#else
                string adUnitId = "unexpected_platform";
#endif

            if (string.IsNullOrEmpty(adUnitId) || AdsMediationManager.instance.isAdsRemoved)
            {
                return;
            }
            // Clean up banner before reusing
            if (bannerView != null)
            {
                DestroyBannerAd();   // delete previous one if present
            }

            // Create a 320x50 banner at top of the screen
            bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);   // for banner shape and its position
                                                                                    // Add Event Handlers
            bannerView.OnBannerAdLoaded += () =>
            {
                print("Banner Ad Loaded");
            };
            bannerView.OnBannerAdLoadFailed += (LoadAdError _error) =>
            {
                print("Banner Failed to loaad with error " + _error.GetMessage());
            };

            bannerView.OnAdImpressionRecorded += () =>
            {
                print("Banner ad recorded an impression.");
            };
            bannerView.OnAdClicked += () =>
            {
                print("Banner ad recorded a click.");
            };
            bannerView.OnAdFullScreenContentOpened += () =>
            {
                print("Banner ad opening.");
            };
            bannerView.OnAdFullScreenContentClosed += () =>
            {
                print("Banner ad closed.");
            };
            bannerView.OnAdPaid += (AdValue adValue) =>
            {
                string msg = string.Format("{0} (currency: {1}, value: {2}",
                                            "Banner ad received a paid event.",
                                            adValue.CurrencyCode,
                                            adValue.Value);
                print(msg);

                AdmobImpressionData _data = new AdmobImpressionData();
                _data.ad_unit_id = bannerView.GetAdUnitID();
                _data.response_id = bannerView.GetResponseInfo().GetResponseId();
                _data.mediation_adapter_class_name = bannerView.GetResponseInfo().GetMediationAdapterClassName();

                _data.currency_code = adValue.CurrencyCode;
                _data.publisher_revenue_decimal = (adValue.Value / 1000000f);
                _data.publisher_revenue_micro = (adValue.Value / 1000000f);
                _data.value_micros = (adValue.Value / 1000000f);
                _data.precision_type = adValue.Precision.ToString();

            };

            var _req = new AdRequest();
            // Load a banner ad
            bannerView.LoadAd(_req);
        }

        public void DestroyBannerAd()
        {
            if (bannerView != null)
            {
                bannerView.Destroy();   // delete previous one if present
                bannerView = null;
            }
        }
        #endregion

        #region INTERSTITIAL ADS
        // call this function to load the Interstitial Ad
        public void RequestAndLoadInterstitialAd()
        {
            print("Requesting Admob Interstitial ad.");

#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = androidInterstitialId;
#elif UNITY_IPHONE
            string adUnitId = iosInterstitialId;
#else
        string adUnitId = "unexpected_platform";
#endif
            if (string.IsNullOrEmpty(adUnitId) || AdsMediationManager.instance.isAdsRemoved)
            {
                return;
            }
            // Clean up interstitial before using it
            DestroyInterstitialAd();

            /*
            * copy from other script
            * this basically tells the admob server that the ads that its requested is going to display inside the unity game, so Admob will choose
            * an Ad accordingly and you can just add more keywords.
            * And it is optional 
           var adRequest = new AdRequest();
           adRequest.Keywords.Add("unity-admob-sample");
           *
           */
            var _req = new AdRequest();
            // Load an interstitial ad
          //  FirebaseManager.instance?.CallFirebasEvent("AdmobInterReq");
            InterstitialAd.Load(adUnitId, _req,
                (InterstitialAd ad, LoadAdError loadError) =>
                {
                    if (loadError != null)
                    {
                        interRetryAttempt++;
                        double retryDelay = Mathf.Pow(2, Mathf.Min(7, interRetryAttempt));
                        Invoke("RequestAndLoadInterstitialAd", (float)retryDelay);

                     //   FirebaseManager.instance?.CallFirebasEvent("AdmobInterFlLoded");

                        print("Interstitial ad failed to load with error: " +
                            loadError.GetMessage());
                        return;
                    }
                    else if (ad == null)    //Failed To Load Inter
                    {
                        print("Interstitial ad failed to load.");
                        interRetryAttempt++;
                        double retryDelay = Mathf.Pow(2, Mathf.Min(7, interRetryAttempt));
                        Invoke("RequestAndLoadInterstitialAd", (float)retryDelay);
                   //     FirebaseManager.instance?.CallFirebasEvent("AdmobInterFlLoded");
                        return;
                    }

                    print("Interstitial ad loaded.");
                    interstitialAd = ad;    // will call this, ShowInterstitialAd()
                    interstitialLoadTime = 180f;
            //        FirebaseManager.instance?.CallFirebasEvent("AdmobInterLoded");
                    ad.OnAdFullScreenContentOpened += () => //On Ad Opened Callback
                    {
                        print("Interstitial ad opening.");
                    };
                    ad.OnAdFullScreenContentClosed += () => //OnAd Closed Callback
                    {
                        print("Interstitial ad closed.");
                        RequestAndLoadInterstitialAd();
              //          FirebaseManager.instance?.CallFirebasEvent("AdmobInterDsply");
                        AdsMediationManager.instance.OnInterstitialClosed();
                        if (AdsMediationManager.instance.sessionFirstInterstitial)
                        {
                            FirebaseManager.instance?.CallFirebasEvent("Usr_Af_FrstInter");
                            AdsMediationManager.instance.sessionFirstInterstitial = false;
                        }
                        // talha.. added
                        //     LoadAdBeforeLoadingTheScene();
                    };
                    ad.OnAdImpressionRecorded += () =>  //On Recorded Impression
                    {
                        print("Interstitial ad recorded an impression.");
                    };
                    ad.OnAdClicked += () => //OnAd Clicked
                    {
                        print("Interstitial ad recorded a click.");
                    };
                    ad.OnAdFullScreenContentFailed += (AdError error) =>    //Inter Ad Failed to show
                    {
                        print("Interstitial ad failed to show with error: " +
                                    error.GetMessage());
                        AdsMediationManager.instance.OnInterstitialClosed();
                        RequestAndLoadInterstitialAd();

                    };
                    ad.OnAdPaid += (AdValue adValue) => //OnAd Paid
                    {
                        string msg = string.Format("{0} (currency: {1}, value: {2}",
                                                   "Interstitial ad received a paid event.",
                                                   adValue.CurrencyCode,
                                                   adValue.Value);
                        print(msg);

                        AdmobImpressionData _data = new AdmobImpressionData();
                        _data.ad_unit_id = ad.GetAdUnitID();
                        _data.response_id = ad.GetResponseInfo().GetResponseId();
                        _data.mediation_adapter_class_name = ad.GetResponseInfo().GetMediationAdapterClassName();

                        _data.currency_code = adValue.CurrencyCode;
                        _data.publisher_revenue_decimal = (adValue.Value / 1000000f);
                        _data.publisher_revenue_micro = (adValue.Value / 1000000f);
                        _data.value_micros = (adValue.Value/1000000f);
                        _data.precision_type = adValue.Precision.ToString();

                    };
                });

        }
        public void ShowInterstitialAd()
        {
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                interstitialAd.Show();
                if (AdsMediationManager.instance.sessionFirstInterstitial)
                {
                    FirebaseManager.instance?.CallFirebasEvent("Usr_BF_FrstInter");
                }
            }
        }

        public void DestroyInterstitialAd()
        {
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }
        }
        public bool IsInterstitialLoaded()
        {
            if (!isInitialized)
                return false;

            return (interstitialAd != null && interstitialAd.CanShowAd());
        }
        #endregion

        #region REWARDED ADS
        // call this function to load the Rewarded Ad
        public void RequestAndLoadRewardedAd()
        {
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = androidRewardedVideoId;
#elif UNITY_IPHONE
        string adUnitId = iosRewardedVideoId;
#else
        string adUnitId = "unexpected_platform";
#endif
            if (string.IsNullOrEmpty(adUnitId))
            {
                return;
            }
            // Clean up RewardedAd before using it
            DestroyRewardedAd();
            //adRequest.Keywords.Add("unity-admob-sample");
            //Loading A Rewarded Ad with Add Event Handlers
            var _req = new AdRequest();
            RewardedAd.Load(adUnitId, _req,
                (RewardedAd ad, LoadAdError loadError) =>
                {
                    if (loadError != null)
                    {
                        print("Rewarded ad failed to load with error: ");
                        print(loadError.GetMessage() + "--" + loadError.GetCode());

                        rvRetryAttempt++;
                        double retryDelay = Mathf.Pow(2, Mathf.Min(7, rvRetryAttempt));
                        Invoke("RequestAndLoadRewardedAd", (float)retryDelay);

                        return;
                    }
                    else if (ad == null)
                    {
                        rvRetryAttempt++;
                        double retryDelay = Mathf.Pow(2, Mathf.Min(7, rvRetryAttempt));
                        Invoke("RequestAndLoadRewardedAd", (float)retryDelay);
                        return;
                    }

                    print("Rewarded ad loaded.");
                    rewardedAd = ad;    // will call this function ShowRewardedAd()

                    // below are all rewarded Ad events
                    rewardedAd.OnAdFullScreenContentOpened += () =>
                        {
                            print("Rewarded ad opening.");


                        };
                    rewardedAd.OnAdFullScreenContentClosed += () =>
                    {
                        print("Rewarded ad closed.");
                        StartCoroutine(CheckForRewardEarned());
                    };
                    rewardedAd.OnAdImpressionRecorded += () =>
                    {
                        print("Rewarded ad recorded an impression.");
                    };
                    rewardedAd.OnAdClicked += () =>
                    {
                        print("Rewarded ad recorded a click.");
                    };
                    rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
                    {
                        print("Rewarded ad failed to show with error: ");
                        if (rvFailEvent != null)
                            rvFailEvent?.Invoke("Failed To Show Ad!");
                        RequestAndLoadRewardedAd();
                    };
                    rewardedAd.OnAdPaid += (AdValue adValue) =>
                    {
                        string msg = string.Format("{0} (currency: {1}, value: {2}",
                                                   "Rewarded ad received a paid event.",
                                                   adValue.CurrencyCode,
                                                   adValue.Value);
                        print(msg);

                        AdmobImpressionData _data = new AdmobImpressionData();
                        _data.ad_unit_id = ad.GetAdUnitID();
                        _data.response_id = ad.GetResponseInfo().GetResponseId();
                        _data.mediation_adapter_class_name = ad.GetResponseInfo().GetMediationAdapterClassName();

                        _data.currency_code = adValue.CurrencyCode;
                        _data.publisher_revenue_decimal = (adValue.Value / 1000000f);
                        _data.publisher_revenue_micro = (adValue.Value / 1000000f);
                        _data.value_micros = (adValue.Value / 1000000f);
                        _data.precision_type = adValue.Precision.ToString();

                    };
                });

        }

        public void ShowRewardedAd(AdsMediationManager.RewardedVieoResponse _successAction = null, AdsMediationManager.RewardedVieoResponse _failureAction = null)
        {
            rvSuccessEvent = _successAction;
            rvFailEvent = _failureAction;
            rvRewardReceived = false;

            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward _reward) =>
                {
                    rvRewardReceived = true;
                });
            }
            else
            {
                if (_failureAction != null)
                    _failureAction?.Invoke("Failed To Load Ad!");
            }
        }
        public bool CanShowRewaredVideo()
        {
            if (!isInitialized)
                return false;

            return (rewardedAd != null && rewardedAd.CanShowAd());
        }
        System.Collections.IEnumerator CheckForRewardEarned()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            print("Checking Reward Earned");
            if (rvRewardReceived)
            {
                if (rvSuccessEvent != null)
                    rvSuccessEvent?.Invoke("Reward Earned!");
                AdsMediationManager.instance.OnRVWatchSuccess();

            }
            else
            {
                if (rvFailEvent != null)
                    rvFailEvent?.Invoke("Reward Failed!");
            }
            rvRewardReceived = false;
            rvSuccessEvent = null;
            rvFailEvent = null;
            RequestAndLoadRewardedAd();
        }

        public void DestroyRewardedAd()
        {
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }
        }
        #endregion

        void OnDestroy()
        {
            CancelInvoke();StopAllCoroutines();
            DestroyRewardedAd();
            if(interstitialAd!=null){interstitialAd.Destroy();interstitialAd=null;}
            if(bannerView!=null){bannerView.Destroy();bannerView=null;}
        }
        public class AdmobImpressionData
        {
            public string ad_unit_id;
            public string currency_code;
            public string response_id;
            public string mediation_adapter_class_name;
            public string precision_type;
            public double publisher_revenue_micro;
            public double publisher_revenue_decimal;
            public double value_micros;
        }

    }
}