using TMPro;
using UnityEngine;
namespace WoolGame {
 public sealed class WoolWalletLabel:MonoBehaviour {
  public TMP_Text label;
  WoolWinPresentation win;int lastBalance=int.MinValue;string formatted;
  void OnEnable(){win=GetComponentInParent<WoolWinPresentation>();lastBalance=int.MinValue;Refresh();}
  void OnTransformParentChanged(){win=GetComponentInParent<WoolWinPresentation>();lastBalance=int.MinValue;}
  void LateUpdate(){Refresh();}
  void Refresh(){
   if(!label||!WoolProgress.IsLoaded)return;
   int balance=win&&win.isActiveAndEnabled&&win.walletLabel==label?win.DisplayBalance:WoolProgress.Coins;
   if(balance!=lastBalance||formatted==null){lastBalance=balance;formatted=WoolCoinText.Format(balance);}if(label.text!=formatted)label.text=formatted;
  }
 }
}
