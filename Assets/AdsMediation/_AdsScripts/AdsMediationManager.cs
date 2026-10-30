using System;
using System.Collections;
using UnityEngine;
namespace AdsMediation {
[RequireComponent(typeof(GoogleMobileAdsConsentController))]
public sealed class AdsMediationManager:MonoBehaviour {
public static AdsMediationManager instance;
public bool isAdsRemoved,displayAdmobBanner,sessionFirstInterstitial=true;
public float interAdDelayTimer=100,InterstitialInterval=100;
public delegate void RewardedVieoResponse(string response);
public event Action<bool> AdVisibilityChanged;
public event Action<bool> AdLoadingChanged;
public const float RewardedStartTimeoutSeconds=5;
bool rewardDisplayed;
bool rewardAppPaused;
Coroutine rewardCloseRoutine;
double rewardStartDeadline;
int rewardRequest;
string rewardPlacement;
bool interDisplayed,interLoadInFlight,rewardLoadInFlight;
double interStartDeadline,interLoadDeadline,rewardLoadDeadline,nextInterLoadAt,nextRewardLoadAt,nextCacheCheck;
int interRequest;
string interPlacement;
MaxAdsConfiguration config;
BaseTenjin attribution;
bool initializing,finished,ready,rewardBusy,interBusy,earned,subscribed;
int interRetry,rewardRetry;
RewardedVieoResponse success,failure,unavailable;
string Inter=>config?config.InterstitialId:"";
string Reward=>config?config.RewardedId:"";
// A missing unit ID follows the unavailable-ad path; it never grants a full reward.
public bool DemoRewardsEnabled=>!config||string.IsNullOrWhiteSpace(config.RewardedId);
void Awake(){if(instance&&instance!=this){Destroy(gameObject);return;}instance=this;DontDestroyOnLoad(gameObject);config=Resources.Load<MaxAdsConfiguration>("ArrowSortMaxAds");}
void Start(){InitializeMediationAds();}
public void InitializeMediationAds(int mediation=1){
if(initializing||ready)return;
if(!config||!config.IsConfigured){finished=true;Debug.LogWarning("MAX setup incomplete: fill SDK key and MAX interstitial/rewarded IDs in ArrowSortMaxAds.asset.");return;}
initializing=true;finished=false;StartCoroutine(Initialize());}
IEnumerator Initialize(){
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
var consent=GetComponent<GoogleMobileAdsConsentController>();bool returned=false;
consent.GatherConsent(error=>{returned=true;if(!string.IsNullOrEmpty(error))Debug.LogWarning("Ads consent: "+error);});
float deadline=Time.realtimeSinceStartup+30;
while(!returned&&Time.realtimeSinceStartup<deadline)yield return null;
if(!returned||!consent.CanRequestAds){Debug.LogWarning("MAX waiting for existing UMP consent setup. Set the real AdMob App ID.");initializing=false;finished=true;yield break;}
#endif
ConnectAttribution();
Subscribe();
if(MaxSdk.IsInitialized())Initialized(null);else MaxSdk.InitializeSdk();
float timeout=Time.realtimeSinceStartup+30;
while(!ready&&Time.realtimeSinceStartup<timeout)yield return null;
initializing=false;finished=true;
}
void ConnectAttribution(){
#if UNITY_ANDROID && !UNITY_EDITOR
if(!config||string.IsNullOrWhiteSpace(config.tenjinSdkKey))return;
if(attribution==null){attribution=Tenjin.getInstance(config.tenjinSdkKey);attribution.SubscribeAppLovinImpressions();}
attribution.Connect();
#endif
}
void OnApplicationPause(bool paused){rewardAppPaused=paused;if(rewardBusy&&!rewardDisplayed)rewardStartDeadline=Time.realtimeSinceStartupAsDouble+RewardedStartTimeoutSeconds;if(paused)return;if(ready){ConnectAttribution();MaintainAdCache();}else InitializeMediationAds();}
void Subscribe(){if(subscribed)return;subscribed=true;
MaxSdkCallbacks.OnSdkInitializedEvent+=Initialized;
MaxSdkCallbacks.Interstitial.OnAdLoadedEvent+=InterLoaded;
MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent+=InterDisplayed;
MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent+=InterLoadFailed;
MaxSdkCallbacks.Interstitial.OnAdHiddenEvent+=InterHidden;
MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent+=InterFailed;
MaxSdkCallbacks.Rewarded.OnAdLoadedEvent+=RewardLoaded;
MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent+=RewardLoadFailed;
MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent+=RewardDisplayed;
MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent+=RewardEarned;
MaxSdkCallbacks.Rewarded.OnAdHiddenEvent+=RewardHidden;
MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent+=RewardFailed;}
void Initialized(MaxSdkBase.SdkConfiguration sdk){if(ready)return;ready=true;finished=true;Debug.Log("AppLovin MAX initialized.");LoadInter();LoadReward();}
// MAX owns one ready cache entry per ad unit. Never issue duplicate loads or
// show a background-loaded ad without a fresh explicit show request.
bool CanStartPreload(bool busy,bool loading,double next,double now)=>ready&&!busy&&!loading&&now>=next&&Application.internetReachability!=NetworkReachability.NotReachable;
void LoadInter(){double now=Time.realtimeSinceStartupAsDouble;if(!CanStartPreload(interBusy,interLoadInFlight,nextInterLoadAt,now)||isAdsRemoved||WoolGame.WoolProgress.NoAdsActive||string.IsNullOrEmpty(Inter)||MaxSdk.IsInterstitialReady(Inter))return;interLoadInFlight=true;interLoadDeadline=now+45;try{MaxSdk.LoadInterstitial(Inter);}catch(Exception e){Debug.LogWarning("MAX background interstitial load: "+e.Message);ScheduleInterRetry(now);}}
void LoadReward(){double now=Time.realtimeSinceStartupAsDouble;if(!CanStartPreload(rewardBusy,rewardLoadInFlight,nextRewardLoadAt,now)||string.IsNullOrEmpty(Reward)||MaxSdk.IsRewardedAdReady(Reward))return;rewardLoadInFlight=true;rewardLoadDeadline=now+45;try{MaxSdk.LoadRewardedAd(Reward);}catch(Exception e){Debug.LogWarning("MAX background rewarded load: "+e.Message);ScheduleRewardRetry(now);}}
void InterLoaded(string id,MaxSdkBase.AdInfo info){if(id==Inter){interLoadInFlight=false;interRetry=0;nextInterLoadAt=0;Debug.Log("MAX interstitial loaded.");}}
void RewardLoaded(string id,MaxSdkBase.AdInfo info){if(id==Reward){rewardLoadInFlight=false;rewardRetry=0;nextRewardLoadAt=0;Debug.Log("MAX rewarded loaded.");}}
void ScheduleInterRetry(double now){interLoadInFlight=false;nextInterLoadAt=now+Mathf.Pow(2,Mathf.Min(6,++interRetry));}
void ScheduleRewardRetry(double now){rewardLoadInFlight=false;nextRewardLoadAt=now+Mathf.Pow(2,Mathf.Min(6,++rewardRetry));}
void InterLoadFailed(string id,MaxSdkBase.ErrorInfo error){if(id!=Inter)return;Debug.LogWarning("MAX interstitial load: "+error.Code+" "+error.Message);ScheduleInterRetry(Time.realtimeSinceStartupAsDouble);}
void RewardLoadFailed(string id,MaxSdkBase.ErrorInfo error){if(id!=Reward)return;Debug.LogWarning("MAX rewarded load: "+error.Code+" "+error.Message);ScheduleRewardRetry(Time.realtimeSinceStartupAsDouble);}
void MaintainAdCache(){double now=Time.realtimeSinceStartupAsDouble;if(interLoadInFlight&&now>=interLoadDeadline)ScheduleInterRetry(now);if(rewardLoadInFlight&&now>=rewardLoadDeadline)ScheduleRewardRetry(now);LoadInter();LoadReward();}
public void CheckForConsent(){InitializeMediationAds();}
public bool IsAdsInitialized()=>ready;
public bool GetInitializationResponse()=>finished;
public bool CanShowInterstitial()=>ready&&!isAdsRemoved&&!WoolGame.WoolProgress.NoAdsActive&&!rewardBusy&&!interBusy&&interAdDelayTimer<=0&&MaxSdk.IsInterstitialReady(Inter);
public void ShowInterstitial(){if(!CanShowInterstitial())return;BeginInterRequest();try{MaxSdk.ShowInterstitial(Inter,interPlacement);}catch(Exception e){Debug.LogException(e);OnInterstitialClosed();LoadInter();}}
void BeginInterRequest(){interBusy=true;interDisplayed=false;interPlacement="level_complete_"+(++interRequest);interStartDeadline=Time.realtimeSinceStartupAsDouble+RewardedStartTimeoutSeconds;interAdDelayTimer=InterstitialInterval;AdLoadingChanged?.Invoke(true);}
bool CurrentInter(string id,MaxSdkBase.AdInfo info)=>interBusy&&id==Inter&&(info==null||string.IsNullOrEmpty(info.Placement)||info.Placement==interPlacement);
void InterDisplayed(string id,MaxSdkBase.AdInfo info){if(!CurrentInter(id,info))return;TickInterStartTimeout(Time.realtimeSinceStartupAsDouble);if(!interBusy)return;interDisplayed=true;AdLoadingChanged?.Invoke(false);AdVisibilityChanged?.Invoke(true);}
void TickInterStartTimeout(double now){if(interBusy&&!interDisplayed&&now>=interStartDeadline){OnInterstitialClosed();LoadInter();}}
public void ShowTestInterstitial(){ShowInterstitial();}
void InterHidden(string id,MaxSdkBase.AdInfo info){if(!CurrentInter(id,info))return;OnInterstitialClosed();LoadInter();sessionFirstInterstitial=false;}
void InterFailed(string id,MaxSdkBase.ErrorInfo error,MaxSdkBase.AdInfo info){if(!CurrentInter(id,info))return;Debug.LogWarning("MAX display: "+error.Message);OnInterstitialClosed();LoadInter();}
public void OnInterstitialClosed(){if(!interBusy)return;interBusy=false;interDisplayed=false;interStartDeadline=0;interAdDelayTimer=InterstitialInterval;AdLoadingChanged?.Invoke(false);AdVisibilityChanged?.Invoke(false);}
public void ShowRewardedVideo(RewardedVieoResponse success=null,RewardedVieoResponse failure=null,RewardedVieoResponse unavailable=null){
if(rewardBusy||interBusy){failure?.Invoke("An ad is already open.");return;}
if(DemoRewardsEnabled){(unavailable??failure)?.Invoke("No rewarded video is available.");return;}
if(!ready||!MaxSdk.IsRewardedAdReady(Reward)){InitializeMediationAds();LoadReward();(unavailable??failure)?.Invoke("No video available. Please try again shortly.");return;}
BeginRewardRequest(success,failure,unavailable);
try{MaxSdk.ShowRewardedAd(Reward,rewardPlacement);}catch(Exception e){Debug.LogException(e);FinishReward("No video available. Please try again.",true);}}
void BeginRewardRequest(RewardedVieoResponse success,RewardedVieoResponse failure,RewardedVieoResponse unavailable){
this.success=success;this.failure=failure;this.unavailable=unavailable;earned=false;rewardBusy=true;rewardDisplayed=false;
rewardPlacement="player_reward_"+(++rewardRequest);rewardStartDeadline=Time.realtimeSinceStartupAsDouble+RewardedStartTimeoutSeconds;AdLoadingChanged?.Invoke(true);}
bool CurrentReward(string id,MaxSdkBase.AdInfo info)=>rewardBusy&&id==Reward&&(info==null||string.IsNullOrEmpty(info.Placement)||info.Placement==rewardPlacement);
void RewardDisplayed(string id,MaxSdkBase.AdInfo info){if(!CurrentReward(id,info))return;rewardDisplayed=true;rewardStartDeadline=0;AdLoadingChanged?.Invoke(false);AdVisibilityChanged?.Invoke(true);}
void TickRewardStartTimeout(double now){if(rewardBusy&&!rewardDisplayed&&!rewardAppPaused&&now>=rewardStartDeadline)FinishReward("No video available. Please try again.",true);}
void RewardEarned(string id,MaxSdkBase.Reward reward,MaxSdkBase.AdInfo info){if(!CurrentReward(id,info))return;earned=true;if(!rewardDisplayed)RewardDisplayed(id,info);}
void RewardHidden(string id,MaxSdkBase.AdInfo info){if(!CurrentReward(id,info)||rewardCloseRoutine!=null)return;rewardCloseRoutine=StartCoroutine(SettleRewardAfterClose(rewardRequest));}
IEnumerator SettleRewardAfterClose(int request){
 yield return null;
 // Some adapters dispatch earned/hidden callbacks in adjacent frames on resume.
 // Keep this request alive briefly so an earned video never becomes compensation.
 for(float t=0;t<.5f&&!earned;t+=Time.unscaledDeltaTime)yield return null;
 rewardCloseRoutine=null;if(rewardBusy&&request==rewardRequest)FinishReward("Video closed before earning the reward.");
}
void RewardFailed(string id,MaxSdkBase.ErrorInfo error,MaxSdkBase.AdInfo info){if(!CurrentReward(id,info))return;Debug.LogWarning("MAX reward display: "+error.Message);FinishReward("No video available. Please try again.",true);}
void FinishReward(string message,bool notAvailable=false){if(!rewardBusy)return;if(rewardCloseRoutine!=null){StopCoroutine(rewardCloseRoutine);rewardCloseRoutine=null;}bool received=earned;var ok=success;var fail=notAvailable?(unavailable??failure):failure;rewardBusy=false;earned=false;rewardDisplayed=false;rewardStartDeadline=0;success=null;failure=null;unavailable=null;interAdDelayTimer=InterstitialInterval;AdLoadingChanged?.Invoke(false);AdVisibilityChanged?.Invoke(false);LoadReward();if(received){OnRVWatchSuccess();ok?.Invoke("Reward Earned!");}else fail?.Invoke(message);}
public void OnRVWatchSuccess(){interAdDelayTimer=InterstitialInterval;WoolGame.WoolProgress.RecordDailyVideo();FirebaseManager.instance?.CallFirebasEvent("rewarded_ad_complete");}
public void OnRemoveAds(){isAdsRemoved=true;RemoveBannerAd();}
public void RemoveBannerAd(){}
public void DisplayBanner(){}
public void DisplayApplovinBannerAd(){DisplayBanner();}
public void HideApplovinBannarAd(){RemoveBannerAd();}
void Update(){interAdDelayTimer=Mathf.Max(0,interAdDelayTimer-Time.unscaledDeltaTime);double now=Time.realtimeSinceStartupAsDouble;TickRewardStartTimeout(now);TickInterStartTimeout(now);if(now>=nextCacheCheck){nextCacheCheck=now+2;MaintainAdCache();}}
void OnDestroy(){CancelInvoke();StopAllCoroutines();if(subscribed){
MaxSdkCallbacks.OnSdkInitializedEvent-=Initialized;
MaxSdkCallbacks.Interstitial.OnAdLoadedEvent-=InterLoaded;
MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent-=InterDisplayed;
MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent-=InterLoadFailed;
MaxSdkCallbacks.Interstitial.OnAdHiddenEvent-=InterHidden;
MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent-=InterFailed;
MaxSdkCallbacks.Rewarded.OnAdLoadedEvent-=RewardLoaded;
MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent-=RewardLoadFailed;
MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent-=RewardDisplayed;
MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent-=RewardEarned;
MaxSdkCallbacks.Rewarded.OnAdHiddenEvent-=RewardHidden;
MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent-=RewardFailed;}
if(instance==this)instance=null;}
}}
