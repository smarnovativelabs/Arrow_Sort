using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed class WoolMapTopBar:MonoBehaviour {
 public WoolSkywayMap map;public WoolHud owner;public RectTransform bar;
 public TMP_Text balance,level;public Button coins,settings,current;
 readonly Vector3[] corners=new Vector3[4];Vector2 lastSize;Rect lastSafe;float nextRefresh;int lastCoins=-1,lastLevel=-1;
 void OnEnable(){if(!coins||!settings||!current||!map||!owner)return;coins.onClick.RemoveAllListeners();coins.onClick.AddListener(owner.ShowCoinOptions);settings.onClick.RemoveAllListeners();settings.onClick.AddListener(()=>owner.ShowAudioSettings(owner.Hide));current.onClick.RemoveAllListeners();current.onClick.AddListener(map.OpenMap);lastSize=Vector2.zero;Refresh();}
 void LateUpdate(){Fit();if(Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.5f;Refresh();}}
 void Refresh(){if(current)current.gameObject.SetActive(false);if(level)level.gameObject.SetActive(false);int value=WoolProgress.Coins,n=WoolProgress.CurrentLevel;if(value!=lastCoins){lastCoins=value;balance.text=WoolCoinText.Format(value);}if(n!=lastLevel){lastLevel=n;level.text="LEVEL "+n;}}
 void Fit(){
  var root=(RectTransform)transform;var size=root.rect.size;var safe=WoolHud.CurrentSafeArea;if(lastSize==size&&lastSafe==safe)return;lastSize=size;lastSafe=safe;
  var canvas=GetComponentInParent<Canvas>();var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
  root.GetWorldCorners(corners);Vector2 min=RectTransformUtility.WorldToScreenPoint(camera,corners[0]),max=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
  float units=size.y/Mathf.Max(1,max.y-min.y),top=Mathf.Max(0,max.y-safe.yMax)*units;
  float scale=Mathf.Min((size.x-36)/900,1.25f);bar.pivot=new Vector2(.5f,.5f);bar.localScale=Vector3.one*scale;bar.anchoredPosition=new Vector2(0,-top-6-58*scale);if(settings)((RectTransform)settings.transform).sizeDelta=Vector2.one*72;
  float bannerTop=top+6+116*scale+30*scale;
  var caption=(RectTransform)map.pageLabel.transform.parent;caption.anchorMin=caption.anchorMax=new Vector2(.5f,1);caption.pivot=new Vector2(.5f,1);caption.anchoredPosition=new Vector2(0,-bannerTop);caption.sizeDelta=new Vector2(size.x*.62f,112*scale);
  for(int i=0;i<2;i++){var r=(RectTransform)(i==0?map.previous:map.next).transform;r.anchoredPosition=new Vector2(i==0?65:-65,-bannerTop);r.sizeDelta=Vector2.one*78*scale;}
 }
}}
