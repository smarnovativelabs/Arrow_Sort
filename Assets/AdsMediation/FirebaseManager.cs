using Firebase;
using Firebase.Extensions;
using UnityEngine;
public sealed class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;
    public bool isInitialized=false;
    bool pending;
    void Awake(){if(instance&&instance!=this){Destroy(this);return;}instance=this;}
    public void InitializeFirebase()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if(pending||isInitialized)return;
        pending=true;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task=>
        {
            if(!this)return;pending=false;
            if(task.IsCanceled||task.IsFaulted||task.Result!=DependencyStatus.Available)
            {Debug.LogWarning("Firebase unavailable; continuing with local settings.");return;}
            isInitialized=true;
            Remoteconfig.instance?.InitializeRemoteConfig();
        });
#endif
    }
    public bool GetFirebaseResponse()=>!pending;
    public void CallFirebasEvent(string eventName)
    {
        if(!isInitialized||string.IsNullOrEmpty(eventName))return;
        Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName.Length>40?eventName.Substring(0,40):eventName);
    }
    public void CallAdImpressionEvent(string name,Firebase.Analytics.Parameter[] parameters)
    {if(isInitialized)Firebase.Analytics.FirebaseAnalytics.LogEvent(name,parameters);}
    void OnDestroy(){if(instance==this)instance=null;}
}
