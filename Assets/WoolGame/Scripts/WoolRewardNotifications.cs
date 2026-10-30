using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
 public sealed class WoolRewardNotifications:MonoBehaviour {
  readonly HashSet<Button> bound=new HashSet<Button>();float next;WoolHud hud;
  void Update(){if(Time.unscaledTime<next)return;next=Time.unscaledTime+1; if(!hud)hud=GetComponent<WoolHud>();if(!hud)return;
   Add(hud.FindHomeTutorialButton("Lucky Spin"),0);Add(hud.FindHomeTutorialButton("Tasks"),2);
   foreach(var nav in GetComponentsInChildren<WoolMenuNavigation>(true))if(nav.tabs!=null&&nav.tabs.Length>1){Add(nav.tabs[0],1);Add(nav.tabs[1],3);}
   // The shop gift already owns its freeCoins notification through
   // WoolHomeRewardBadges. Adding another here drew two ready icons.
   if(hud.TutorialMap){Add(hud.TutorialMap.shop,1);Add(hud.TutorialMap.home,3);}
  }
  void Add(Button button,int kind){if(!button||!bound.Add(button))return;var root=new GameObject("Ready reward notification",typeof(RectTransform),typeof(CanvasRenderer),typeof(WoolRewardBadge));var rt=(RectTransform)root.transform;rt.SetParent(button.transform,false);rt.anchorMin=rt.anchorMax=new Vector2(.88f,.90f);rt.sizeDelta=new Vector2(48,48);var badge=root.GetComponent<WoolRewardBadge>();badge.kind=kind;badge.color=new Color(.93f,.12f,.08f);badge.raycastTarget=false;
   var label=new GameObject("Ready",typeof(RectTransform),typeof(TextMeshProUGUI));var lr=(RectTransform)label.transform;lr.SetParent(rt,false);lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;badge.label=label.GetComponent<TMP_Text>();badge.label.font=TMP_Settings.defaultFontAsset;badge.label.fontSize=34;badge.label.fontStyle=FontStyles.Bold;badge.label.alignment=TextAlignmentOptions.Center;badge.label.color=Color.white;badge.label.raycastTarget=false;
  }
 }
 public sealed class WoolRewardBadge:MaskableGraphic {
  public int kind;public TMP_Text label;float next;
  public static bool DailyReady(){for(int i=0;i<WoolProgress.DailyTargets.Length;i++)if(!WoolProgress.DailyClaimed(i)&&WoolProgress.DailyCount(i)>=WoolProgress.DailyTargets[i])return true;for(int i=0;i<4;i++)if(!WoolProgress.DailyGiftClaimed(i)&&WoolProgress.DailyCredits>=(i+1)*25)return true;return WoolProgress.DailyChestCredits>=700&&!WoolProgress.DailyChestCollected;}
  protected override void OnEnable(){base.OnEnable();Refresh();}
  void Update(){if(Time.unscaledTime<next)return;next=Time.unscaledTime+.2f;Refresh();}
  void Refresh(){if(!label)return;int spins=WoolProgress.ReadySpinNotifications;bool ready=kind==0?spins>0:kind==1?WoolProgress.CanClaimShopDailyGift:kind==2?DailyReady():spins>0||DailyReady();canvasRenderer.SetAlpha(ready?1:0);label.enabled=ready;label.text=kind==0?Mathf.Min(99,spins).ToString():"!";}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;var center=r.center;float radius=Mathf.Min(r.width,r.height)*.5f;vh.AddVert(center,color,Vector2.zero);for(int i=0;i<=32;i++){float a=i*Mathf.PI*2/32;vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);if(i>0)vh.AddTriangle(0,i,i+1);}}
 }
}
