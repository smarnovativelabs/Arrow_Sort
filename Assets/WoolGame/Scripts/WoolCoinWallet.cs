using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
// Shared balance treatment. Reward amounts and prices remain separate from the wallet.
public sealed class WoolCoinWallet:MonoBehaviour {
 public WoolHud owner;public TMP_Text amount;public Button button;public Image coin;public TMP_Text plus;
 void OnEnable(){Bind();Refresh();}
 void Start(){Bind();}
 void Bind(){if(!button||!owner)return;button.onClick.RemoveAllListeners();for(int i=0;i<button.onClick.GetPersistentEventCount();i++)button.onClick.SetPersistentListenerState(i,UnityEngine.Events.UnityEventCallState.Off);button.onClick.AddListener(owner.ShowCoinOptions);}
 void LateUpdate(){Refresh();}
 public void Refresh(){if(!amount)return;amount.text=WoolCoinText.Format(WoolProgress.Coins);amount.color=WoolArt.Hex("693415");var r=(RectTransform)transform;float w=r.rect.width,h=r.rect.height;if(w<=0||h<=0)return;var t=amount.rectTransform;t.anchorMin=t.anchorMax=new Vector2(.5f,.5f);t.pivot=new Vector2(.5f,.5f);t.anchoredPosition=new Vector2(h*.14f,0);t.sizeDelta=new Vector2(Mathf.Max(40,w-h*1.5f),h*.76f);amount.enableAutoSizing=true;amount.fontSizeMax=h*.5f;amount.fontSizeMin=Mathf.Max(18,h*.25f);amount.textWrappingMode=TextWrappingModes.NoWrap;if(coin){coin.rectTransform.anchoredPosition=new Vector2(-w*.5f+h*.47f,0);coin.rectTransform.sizeDelta=Vector2.one*h*.97f;}if(plus){plus.rectTransform.anchoredPosition=new Vector2(w*.5f-h*.32f,0);plus.rectTransform.sizeDelta=new Vector2(h*.45f,h*.75f);plus.fontSize=h*.46f;plus.color=WoolArt.Hex("3BA940");}}
 public void MatchAppearance(WoolCoinWallet source){
  if(!source||source==this)return;var image=GetComponent<Image>();var original=source.GetComponent<Image>();
  if(image&&original){image.enabled=true;image.sprite=original.sprite;image.type=original.type;image.preserveAspect=original.preserveAspect;image.color=original.color;image.raycastTarget=true;}
  ((RectTransform)transform).sizeDelta=((RectTransform)source.transform).sizeDelta;
  if(amount&&source.amount){amount.font=source.amount.font;amount.fontSharedMaterial=source.amount.fontSharedMaterial;amount.color=source.amount.color;amount.gameObject.SetActive(true);}
  if(coin&&source.coin){coin.sprite=source.coin.sprite;coin.color=source.coin.color;coin.preserveAspect=source.coin.preserveAspect;coin.enabled=true;coin.gameObject.SetActive(true);}
  if(plus&&source.plus){plus.text=source.plus.text;plus.font=source.plus.font;plus.fontSharedMaterial=source.plus.fontSharedMaterial;plus.gameObject.SetActive(true);}
  if(button&&source.button){button.colors=source.button.colors;button.transition=source.button.transition;button.targetGraphic=image;}Refresh();
 }
}
public sealed partial class WoolHud {
 public WoolCoinWallet HomeWallet=>homeCoins?homeCoins.GetComponentInParent<WoolCoinWallet>(true):null;
 bool walletShopOpen;Button walletShopBack;
 void ShowWalletShop(){
 if(menu.activeSelf){CloseEngagement();Hide();ShowShop();var shop=shopPage.GetComponent<WoolShopView>();if(shop)shop.FocusCoinPacks();return;}
 bool priorModal=modal.activeSelf;utilityPanel.Hide();modal.SetActive(false);walletShopOpen=true;menu.SetActive(true);header.gameObject.SetActive(false);footer.gameObject.SetActive(false);ShowShop();
 if(!walletShopBack){var root=Rect("Coin shop return controls",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var layer=root.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=510;root.gameObject.AddComponent<GraphicRaycaster>();walletShopBack=ButtonAt("Return from coin shop",root,"BACK TO GAME",new Vector2(340,70),Vector2.zero,null);var im=walletShopBack.GetComponent<Image>();im.sprite=skin[1];im.color=Color.white;}
 var rt=(RectTransform)walletShopBack.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.anchoredPosition=new Vector2(0,-225);walletShopBack.gameObject.SetActive(true);walletShopBack.onClick.RemoveAllListeners();walletShopBack.onClick.AddListener(()=>{walletShopOpen=false;walletShopBack.gameObject.SetActive(false);CloseMenu();modal.SetActive(priorModal);});var view=shopPage.GetComponent<WoolShopView>();if(view)view.FocusCoinPacks();
 }
 void ResetWalletShop(){walletShopOpen=false;if(walletShopBack)walletShopBack.gameObject.SetActive(false);}
}
}
