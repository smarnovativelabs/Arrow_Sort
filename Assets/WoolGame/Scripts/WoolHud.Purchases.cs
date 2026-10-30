using UnityEngine;
namespace WoolGame {
public sealed partial class WoolHud {
 WoolPurchases purchaseUI;
 void BindPurchaseUI(){if(purchaseUI)return;purchaseUI=WoolPurchases.Ensure();purchaseUI.Message+=PurchaseMessage;}
 public void BuyShopProduct(string id){BindPurchaseUI();purchaseUI.Buy(id);}
 void PurchaseMessage(string title,string body){RefreshWalletLabels();var shop=GetComponentInChildren<WoolShopView>(true);if(shop)shop.Refresh();CloseEngagement();Show(title,body,"BACK",Hide);}
 void OnDestroy(){if(purchaseUI)purchaseUI.Message-=PurchaseMessage;}
}
}
