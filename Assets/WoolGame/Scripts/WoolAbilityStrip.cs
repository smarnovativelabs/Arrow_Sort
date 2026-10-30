using TMPro;
using UnityEngine;
namespace WoolGame {
public sealed class WoolAbilityStrip:MonoBehaviour {
 public TMP_Text[] counts;int timedSeconds;
 void OnEnable(){Refresh();}
 public void Refresh(){if(counts==null||!WoolProgress.IsLoaded)return;for(int i=0;i<counts.Length;i++){int n=WoolProgress.AbilityCount(i);counts[i].text=i==3&&timedSeconds>0?timedSeconds+"s":n>0?n.ToString():"+";}}
 public void SetCountdown(float remaining){int value=Mathf.CeilToInt(remaining);if(value==timedSeconds&&value==0)return;timedSeconds=value;if(value>0&&counts!=null&&counts.Length>3)counts[3].text=value+"s";else Refresh();}
}
public sealed partial class WoolHud {
 [SerializeField] WoolAbilityStrip abilityStrip;
 public void RefreshAbilityInventory(){if(abilityStrip)abilityStrip.Refresh();if(abilityStage>0)SetAbilityLevels(abilityStage);}
 public void SetAmplifyCountdown(float remaining){if(abilityStrip)abilityStrip.SetCountdown(remaining);}
}
}
