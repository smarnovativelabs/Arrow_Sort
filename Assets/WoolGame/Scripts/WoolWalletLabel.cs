using TMPro;
using UnityEngine;
namespace WoolGame {
 public sealed class WoolWalletLabel:MonoBehaviour {
  public TMP_Text label;
  void OnEnable(){Refresh();}
  void LateUpdate(){Refresh();}
  void Refresh(){if(!label||!WoolProgress.IsLoaded)return;string value=WoolCoinText.Format(WoolProgress.Coins);if(label.text!=value)label.text=value;}
 }
}