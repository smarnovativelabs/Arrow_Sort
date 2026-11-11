using TMPro;
using UnityEngine;
namespace WoolGame {
public sealed class WoolAbilityStrip:MonoBehaviour {
 public TMP_Text[] counts;int timedSeconds;Vector2 normalBadgeSize;float normalFontSize;
 void OnEnable(){Refresh();}
 public void Refresh(){if(counts==null||!WoolProgress.IsLoaded)return;for(int i=0;i<counts.Length;i++){counts[i].text=BadgeText(i);}FitTimerBadge();}
 public string BadgeText(int kind){if(kind==3&&timedSeconds>0)return TimerText();int count=WoolProgress.AbilityCount(kind);return count>0?count.ToString():"+";}
 string TimerText()=>timedSeconds>=60?(timedSeconds/60)+":"+(timedSeconds%60).ToString("D2"):timedSeconds+"s";
 public void SetCountdown(float remaining){int value=Mathf.Max(0,Mathf.CeilToInt(remaining));if(value==timedSeconds)return;timedSeconds=value;Refresh();}
 void FitTimerBadge(){
  if(counts==null||counts.Length<4||!counts[3])return;
  var badge=counts[3].transform.parent as RectTransform;if(!badge)return;
  if(normalBadgeSize==Vector2.zero){var reference=counts[0]?counts[0].transform.parent as RectTransform:null;normalBadgeSize=reference?reference.sizeDelta:new Vector2(64,64);normalFontSize=counts[0]?counts[0].fontSize:44;}
  bool active=timedSeconds>0;badge.sizeDelta=active?new Vector2(Mathf.Max(normalBadgeSize.x,112),normalBadgeSize.y):normalBadgeSize;
  var text=counts[3];var rect=text.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(5,2);rect.offsetMax=new Vector2(-5,-2);
  text.alignment=TextAlignmentOptions.Center;text.enableAutoSizing=true;text.fontSize=text.fontSizeMax=active?Mathf.Min(normalFontSize,42):normalFontSize;text.fontSizeMin=active?26:normalFontSize*.8f;text.textWrappingMode=TextWrappingModes.NoWrap;
 }
}
public sealed partial class WoolHud {
 [SerializeField] WoolAbilityStrip abilityStrip;
 public void RefreshAbilityInventory(){if(abilityStrip)abilityStrip.Refresh();if(abilityStage>0)SetAbilityLevels(abilityStage);}
 public void SetAmplifyCountdown(float remaining){if(abilityStrip)abilityStrip.SetCountdown(remaining);}
}
}
