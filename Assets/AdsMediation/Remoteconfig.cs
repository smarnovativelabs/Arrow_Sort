using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using UnityEngine;
public sealed class Remoteconfig : MonoBehaviour
{
    public static Remoteconfig instance;
    bool pending;
    void Awake(){instance=this;}
    public void InitializeRemoteConfig(){if(!pending)StartCoroutine(Fetch());}
    IEnumerator Fetch()
    {
        pending=true;
        var config=FirebaseRemoteConfig.DefaultInstance;
        var defaults=config.SetDefaultsAsync(new Dictionary<string,object>{{"interstitialDelay",100L}});
        float deadline=Time.realtimeSinceStartup+15;
        while(!defaults.IsCompleted&&Time.realtimeSinceStartup<deadline)yield return null;
        if(!defaults.IsCompleted||defaults.IsFaulted||defaults.IsCanceled){pending=false;yield break;}
        var fetch=config.FetchAsync(TimeSpan.FromHours(12));
        while(!fetch.IsCompleted&&Time.realtimeSinceStartup<deadline)yield return null;
        if(!fetch.IsCompleted||fetch.IsFaulted||fetch.IsCanceled){pending=false;yield break;}
        var activate=config.ActivateAsync();
        while(!activate.IsCompleted&&Time.realtimeSinceStartup<deadline)yield return null;
        if(activate.IsCompleted&&!activate.IsFaulted&&!activate.IsCanceled)OnDataFetched();
        pending=false;
    }
    public void OnDataFetched()
    {
        if(AdsMediation.AdsMediationManager.instance)
            AdsMediation.AdsMediationManager.instance.InterstitialInterval=Mathf.Clamp((float)FirebaseRemoteConfig.DefaultInstance.GetValue("interstitialDelay").LongValue,60,600);
    }
    void OnDestroy(){if(instance==this)instance=null;}
}
