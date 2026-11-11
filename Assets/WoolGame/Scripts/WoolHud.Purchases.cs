using UnityEngine;
using TMPro;
namespace WoolGame {
public sealed partial class WoolHud {
 WoolPurchases purchaseUI;
 RectTransform purchaseResultPanel;TMP_Text purchaseSuccessText;float purchaseSuccessUntil;
 void BindPurchaseUI(){if(purchaseUI)return;purchaseUI=WoolPurchases.Ensure();purchaseUI.Message+=PurchaseMessage;}
 public void BuyShopProduct(string id){BindPurchaseUI();purchaseUI.Buy(id);}
 void PurchaseMessage(string title,string body){RefreshWalletLabels();var shop=GetComponentInChildren<WoolShopView>(true);if(shop)shop.Refresh();if(title=="PURCHASE COMPLETE"||title=="TEST PURCHASE COMPLETE"){ShowPurchaseResult("Purchased Successfully!");return;}if(title=="STORE UNAVAILABLE"||title=="PURCHASE UNAVAILABLE"||title=="PURCHASE NOT COMPLETED"||title=="PURCHASE PENDING"||title=="PURCHASE AWAITING APPROVAL"){ShowPurchaseResult(title=="PURCHASE PENDING"||title=="PURCHASE AWAITING APPROVAL"?"Purchase Pending":"Purchase Failed!");return;}CloseEngagement();Show(title,body,"BACK",Hide);}
 void ShowPurchaseResult(string message){
  if(!purchaseSuccessText){
   if(purchaseResultPanel)Destroy(purchaseResultPanel.gameObject);
   var panel=new GameObject("Purchase result overlay",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(Canvas),typeof(UnityEngine.UI.GraphicRaycaster));
   purchaseResultPanel=panel.GetComponent<RectTransform>();purchaseResultPanel.SetParent(canvas.transform,false);purchaseResultPanel.anchorMin=Vector2.zero;purchaseResultPanel.anchorMax=Vector2.one;purchaseResultPanel.offsetMin=purchaseResultPanel.offsetMax=Vector2.zero;
   var overlay=panel.GetComponent<Canvas>();overlay.overrideSorting=true;overlay.sortingLayerID=canvas.sortingLayerID;overlay.sortingOrder=32000;
   panel.GetComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,.65f);
   float width=Mathf.Min(780,((RectTransform)canvas.transform).rect.width-64);
   var card=Panel("Purchase result card",purchaseResultPanel,new Vector2(.5f,.5f),new Vector2(width,300),Vector2.zero,Color.white);
   var face=card.GetComponent<UnityEngine.UI.Image>();if(utilityPanel&&utilityPanel.card){var source=utilityPanel.card.GetComponent<UnityEngine.UI.Image>();if(source){face.sprite=source.sprite;face.type=source.type;}else{face.sprite=skin!=null&&skin.Length>0?skin[0]:rounded;face.type=UnityEngine.UI.Image.Type.Sliced;}}else{face.sprite=skin[0];face.type=UnityEngine.UI.Image.Type.Sliced;}
   var plate=Panel("Purchase title plate",card,new Vector2(.5f,.5f),new Vector2(width-100,110),new Vector2(0,140),Color.white);
   plate.GetComponent<UnityEngine.UI.Image>().sprite=Resources.Load<Sprite>("WoolButtons/Amber");plate.GetComponent<UnityEngine.UI.Image>().type=UnityEngine.UI.Image.Type.Sliced;
   purchaseSuccessText=Label("Purchase result title",plate,"",48,new Vector2(width-170,90),Vector2.zero,Color.white);purchaseSuccessText.outlineColor=WoolArt.Hex("713415");purchaseSuccessText.outlineWidth=.15f;purchaseSuccessText.enableAutoSizing=true;purchaseSuccessText.fontSizeMin=30;purchaseSuccessText.fontSizeMax=48;purchaseSuccessText.raycastTarget=false;
   Label("Purchase result description",card,"",38,new Vector2(width-100,130),new Vector2(0,-15),WoolArt.Hex("713415"));

  }
  bool success=message=="Purchased Successfully!";
  bool pending=message=="Purchase Pending";purchaseSuccessText.text=success?"Purchase Successful!":pending?"Purchase Pending":"Purchase Failed!";
  var description=purchaseResultPanel.Find("Purchase result card/Purchase result description").GetComponent<TMP_Text>();description.text=success?"Your items have been added!":pending?"Your purchase is awaiting confirmation.":"No items were added.";
  purchaseResultPanel.SetAsLastSibling();purchaseResultPanel.gameObject.SetActive(true);purchaseSuccessUntil=Time.realtimeSinceStartup+2;
 }

 void HidePurchaseResult(){if(purchaseResultPanel)purchaseResultPanel.gameObject.SetActive(false);}
 void UpdatePurchaseSuccess(){if(purchaseResultPanel&&purchaseResultPanel.gameObject.activeSelf&&Time.realtimeSinceStartup>=purchaseSuccessUntil)HidePurchaseResult();}
 void OnDestroy(){if(purchaseUI)purchaseUI.Message-=PurchaseMessage;}
}
}
