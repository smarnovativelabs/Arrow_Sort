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
 bool walletShopOpen;Button walletShopBack;System.Action walletShopReturn;
 bool walletPriorModal,walletPriorHeader,walletPriorFooter,walletPriorUtility,walletPriorHint;
 void ShowWalletShop(){
  if(walletShopOpen)return;
  if(menu.activeSelf){CloseEngagement();Hide();ShowShop();var shop=shopPage.GetComponent<WoolShopView>();if(shop)shop.FocusCoinPacks();return;}
  ShowGameplayCoinShop();
 }
 public void ShowGameplayCoinShop(System.Action onReturn=null){
  if(walletShopOpen)return;
  walletPriorModal=modal.activeSelf;walletPriorHeader=header.gameObject.activeSelf;
  walletPriorFooter=footer.gameObject.activeSelf;walletPriorHint=hint.gameObject.activeSelf;
  walletPriorUtility=utilityPanel&&utilityPanel.gameObject.activeSelf;walletShopReturn=onReturn;
  if(utilityPanel)utilityPanel.Hide();modal.SetActive(false);walletShopOpen=true;
  menu.SetActive(true);header.gameObject.SetActive(false);footer.gameObject.SetActive(false);hint.gameObject.SetActive(false);ShowShop();FitUnifiedNavigation();
  if(!walletShopBack){
   var root=Rect("Coin shop return controls",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
   var layer=root.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=510;
   root.gameObject.AddComponent<GraphicRaycaster>();
   walletShopBack=ButtonAt("Close gameplay coin shop",root,"\u00D7",Vector2.one*88,Vector2.zero,CloseGameplayCoinShop);
   var image=walletShopBack.GetComponent<Image>();image.sprite=skin[3];image.color=Color.white;
   var label=walletShopBack.GetComponentInChildren<TMP_Text>();label.fontSize=64;label.alignment=TextAlignmentOptions.Center;
  }
  var rt=(RectTransform)walletShopBack.transform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(.5f,.5f);
  float scale=GetComponent<Canvas>().scaleFactor;var area=CurrentSafeArea;
  rt.anchoredPosition=new Vector2(64+area.xMin/Mathf.Max(.01f,scale),-64-(Screen.height-area.yMax)/Mathf.Max(.01f,scale));
  walletShopBack.gameObject.SetActive(true);walletShopBack.transform.parent.SetAsLastSibling();
  var view=shopPage.GetComponent<WoolShopView>();if(view)view.FocusCoinPacks();
 }
 public void CloseGameplayCoinShop(){
  if(!walletShopOpen)return;
  var returned=walletShopReturn;walletShopReturn=null;walletShopOpen=false;
  walletShopBack.gameObject.SetActive(false);if(utilityPanel)utilityPanel.Hide();menu.SetActive(false);
  header.gameObject.SetActive(walletPriorHeader);footer.gameObject.SetActive(walletPriorFooter);hint.gameObject.SetActive(walletPriorHint);
  modal.SetActive(returned==null&&walletPriorModal);
  if(returned!=null)returned();else if(walletPriorUtility&&utilityPanel)utilityPanel.gameObject.SetActive(true);
  gameplayInputBlockedThroughFrame=Time.frameCount+1;RefreshWalletLabels();SetDanger(requestedDangerIntensity,requestedDangerTime);
 }
 void ResetWalletShop(){walletShopOpen=false;walletShopReturn=null;if(walletShopBack)walletShopBack.gameObject.SetActive(false);}
}
}