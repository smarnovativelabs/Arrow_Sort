using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed class WoolUtilityPanel:MonoBehaviour {
 public RectTransform card;public GameObject settingsBody,coinsBody;
 public TMP_Text title,balance,musicState,soundState,dailyState;
 public Button close,music,sound,daily,video,shop;
 public Sprite greenButton,greyButton;
 WoolGameAudio audio;Action closed,walletChanged;Vector2 lastSize;Rect lastSafe;float nextRefresh;bool coinMode;
 Button gameplayHome;
 public void ShowSettings(WoolGameAudio gameAudio,Action back,Action home=null){
  audio=gameAudio;closed=back;coinMode=false;title.text="Settings";settingsBody.SetActive(true);coinsBody.SetActive(false);
  music.onClick.RemoveAllListeners();music.onClick.AddListener(()=>{audio.SetMusic(!audio.MusicEnabled);Refresh();});
  sound.onClick.RemoveAllListeners();sound.onClick.AddListener(()=>{audio.SetEffects(!audio.EffectsEnabled);Refresh();});
  var done=settingsBody.transform.Find("Done") as RectTransform;
  if(done){
   done.sizeDelta=new Vector2(home!=null?320:450,105);done.anchoredPosition=new Vector2(home!=null?-180:0,-390);
   if(home!=null&&!gameplayHome){gameplayHome=Instantiate(done.gameObject,done.parent).GetComponent<Button>();gameplayHome.name="Gameplay Home";}
   if(gameplayHome){gameplayHome.gameObject.SetActive(home!=null);if(home!=null){var rt=(RectTransform)gameplayHome.transform;rt.anchoredPosition=new Vector2(180,-390);rt.sizeDelta=new Vector2(320,105);gameplayHome.GetComponentInChildren<TMP_Text>().text="HOME";gameplayHome.onClick.RemoveAllListeners();gameplayHome.onClick.AddListener(()=>{Hide();home();});}}
  }
  Open();
 }
 public void ShowCoins(Action buy,Action watch,Action changed){
  coinMode=true;closed=null;walletChanged=changed;title.text="Your Coins";settingsBody.SetActive(false);coinsBody.SetActive(true);
  daily.onClick.RemoveAllListeners();daily.onClick.AddListener(()=>{if(WoolProgress.ClaimShopDailyGift()){walletChanged?.Invoke();var a=FindFirstObjectByType<WoolGameAudio>();if(a)a.Play(WoolGameAudio.Cue.Coin);}Refresh();});
  video.onClick.RemoveAllListeners();video.onClick.AddListener(()=>{Hide();watch?.Invoke();});
  shop.onClick.RemoveAllListeners();shop.onClick.AddListener(()=>{Hide();buy?.Invoke();});Open();
 }
 void Open(){close.onClick.RemoveAllListeners();close.onClick.AddListener(()=>{Hide();closed?.Invoke();});gameObject.SetActive(true);transform.SetAsLastSibling();lastSize=Vector2.zero;Fit();Refresh();}
 public void Dismiss(){Hide();closed?.Invoke();}
 public void Hide(){gameObject.SetActive(false);}
 void Refresh(){
  if(coinMode){balance.text=WoolProgress.Coins.ToString("N0");bool available=WoolProgress.CanClaimShopDailyGift;daily.interactable=available;dailyState.text=available?"CLAIM +50":"COLLECTED";daily.GetComponent<Image>().sprite=available?greenButton:greyButton;}
  else if(audio){musicState.text=audio.MusicEnabled?"ON":"OFF";soundState.text=audio.EffectsEnabled?"ON":"OFF";music.GetComponent<Image>().sprite=audio.MusicEnabled?greenButton:greyButton;sound.GetComponent<Image>().sprite=audio.EffectsEnabled?greenButton:greyButton;}
 }
 void Update(){Fit();if(Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.5f;Refresh();}}
 void Fit(){
  var root=(RectTransform)transform;var size=root.rect.size;var safe=WoolHud.CurrentSafeArea;if(size==lastSize&&safe==lastSafe)return;lastSize=size;lastSafe=safe;
  // This panel already inherits the HUD safe rectangle. Do not apply the notch twice.
  float top=0,bottom=0,left=0,right=0;
  card.localScale=Vector3.one*Mathf.Max(.1f,Mathf.Min(1.18f,Mathf.Min((size.x-left-right-48)/860,(size.y-top-bottom-48)/1000)));
  card.anchoredPosition=new Vector2((left-right)*.5f,(bottom-top)*.5f);
 }
}}
