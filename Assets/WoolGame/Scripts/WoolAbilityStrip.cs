using TMPro;
using UnityEngine;
namespace WoolGame {
public sealed class WoolAbilityStrip:MonoBehaviour {
 public TMP_Text[] counts;
 void OnEnable(){Refresh();}
 public void Refresh(){if(counts==null||!WoolProgress.IsLoaded)return;for(int i=0;i<counts.Length;i++){int n=WoolProgress.AbilityCount(i);counts[i].text=n>0?n.ToString():"+";}}
}
public sealed partial class WoolHud {
 [SerializeField] WoolAbilityStrip abilityStrip;
 public void RefreshAbilityInventory(){if(abilityStrip)abilityStrip.Refresh();if(abilityStage>0)SetAbilityLevels(abilityStage);}
}
}
