using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
namespace WoolGame {
public sealed class WoolLosePresentation:MonoBehaviour {
 public RectTransform card,hero;
 public RectTransform background;
 public Button continueButton,retry,home;
 public Image[] tears;
 public Vector2[] eyePositions;
 public GameObject offerPage,retryPage;
 public Button coinButton,cancelOffer;
 public TMP_Text balance;
 public RectTransform smilingDragon;
 public Sprite[] characterFrames;
 public Image animatedCharacters,backdrop;
 public RectTransform retryBanner,retryCaption,wallet;
 public TMP_Text affordability;
 public const int ClearCost=1200;
 float clock; bool selected;
 Material retryTextMaterial;
 void RestoreRetryLabel(){
  TMP_Text text=null;foreach(var t in retry.GetComponentsInChildren<TMP_Text>(true)){text=t;break;}
  if(!text){var go=new GameObject("Visible retry label",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(retry.transform,false);text=go.GetComponent<TMP_Text>();}
  text.gameObject.SetActive(true);text.enabled=true;text.text="RETRY";text.color=Color.white;text.raycastTarget=false;text.alignment=TextAlignmentOptions.Center;text.enableAutoSizing=true;text.fontSize=text.fontSizeMax=64;text.fontSizeMin=48;text.textWrappingMode=TextWrappingModes.NoWrap;
  if(!retryTextMaterial&&text.fontSharedMaterial){retryTextMaterial=new Material(text.fontSharedMaterial);retryTextMaterial.SetColor("_FaceColor",Color.white);retryTextMaterial.SetColor("_OutlineColor",new Color(.1f,.25f,.04f));retryTextMaterial.SetFloat("_OutlineWidth",.14f);retryTextMaterial.EnableKeyword("OUTLINE_ON");}if(retryTextMaterial)text.fontSharedMaterial=retryTextMaterial;
  var rect=text.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(24,8);rect.offsetMax=new Vector2(-24,-8);rect.localScale=Vector3.one;text.transform.SetAsLastSibling();text.canvasRenderer.SetAlpha(1);
 }
 void OnDestroy(){if(retryTextMaterial)Destroy(retryTextMaterial);}
 public void Open(bool canContinue,Action resume,Action restart,Action goHome){
 RestoreRetryLabel();
 clock=0;selected=false;continueButton.gameObject.SetActive(canContinue);
 if(offerPage){offerPage.SetActive(canContinue);retryPage.SetActive(!canContinue);if(backdrop)backdrop.color=Color.black;balance.text=WoolCoinText.Format(WoolProgress.Coins);coinButton.interactable=WoolProgress.Coins>=ClearCost;coinButton.onClick.RemoveAllListeners();coinButton.onClick.AddListener(()=>{if(selected)return;selected=true;coinButton.interactable=false;resumeCoins?.Invoke();});cancelOffer.onClick.RemoveAllListeners();cancelOffer.onClick.AddListener(()=>{if(selected)return;offerPage.SetActive(false);retryPage.SetActive(true);if(backdrop)backdrop.color=Color.black;});}
 Bind(continueButton,resume);Bind(retry,restart);Bind(home,goHome);Fit();
 }
 public Action resumeCoins;
 void Bind(Button b,Action action){b.onClick.RemoveAllListeners();b.interactable=true;b.onClick.AddListener(()=>{if(selected)return;selected=true;continueButton.interactable=retry.interactable=home.interactable=false;action?.Invoke();});}
 void Fit(){var area=Screen.safeArea;float s=GetComponentInParent<Canvas>().scaleFactor;float w=area.width/s,h=area.height/s;float scale=Mathf.Max(.1f,Mathf.Min((w-40)/760,(h-60)/1320));card.localScale=Vector3.one*scale;card.anchoredPosition=(area.center-new Vector2(Screen.width,Screen.height)*.5f)/s;
 if(retryBanner){float half=h/scale*.5f;retryBanner.anchoredPosition=new Vector2(0,half-245);retryCaption.anchoredPosition=new Vector2(0,-half+340);((RectTransform)retry.transform).anchoredPosition=new Vector2(0,-half+175);((RectTransform)home.transform).anchoredPosition=new Vector2(-310,half-85);hero.anchoredPosition=new Vector2(0,Mathf.Clamp((half-700)*.08f,0,70));hero.sizeDelta=new Vector2(730,Mathf.Min(850,half*2-790));if(wallet)wallet.anchoredPosition=new Vector2(190,half-105);}
 if(background)background.gameObject.SetActive(false);}
 void Update(){Fit();clock+=Mathf.Min(Time.unscaledDeltaTime,.05f);hero.localRotation=Quaternion.Euler(0,0,Mathf.Sin(clock*1.8f)*1.2f);
 if(backdrop)backdrop.color=Color.black;
 if(offerPage&&offerPage.activeSelf){int coins=WoolProgress.Coins;balance.text=WoolCoinText.Format(coins);if(affordability){affordability.text=coins>=ClearCost?"Enough coins to clear slots":"Need "+WoolCoinText.Format(ClearCost-coins)+" more coins";affordability.color=coins>=ClearCost?new Color(.5f,1,.45f):new Color(1,.72f,.42f);}coinButton.interactable=!selected&&coins>=ClearCost;}
 if(animatedCharacters&&characterFrames!=null&&characterFrames.Length>0)animatedCharacters.sprite=characterFrames[(int)(clock*8)%characterFrames.Length];
 if(smilingDragon){smilingDragon.localRotation=Quaternion.Euler(0,0,Mathf.Sin(clock*2.2f)*3);smilingDragon.localScale=new Vector3(1,1+Mathf.Sin(clock*3)*.025f,1);}
 for(int i=0;i<tears.Length;i++){float t=Mathf.Repeat(clock*.5f+i*.25f,1);var im=tears[i];im.rectTransform.anchoredPosition=eyePositions[i%eyePositions.Length]+new Vector2((i%2==0?-1:1)*t*5,-(t<.7f?t/.7f*40:40+Mathf.Pow((t-.7f)/.3f,2)*35));im.rectTransform.localScale=Vector3.one*(.5f+Mathf.Sin(t*Mathf.PI)*.5f);im.color=new Color(.85f,.96f,1,Mathf.Min(1,t*12)*Mathf.Clamp01((1-t)*6)*.88f);}
 }
}
public sealed partial class WoolHud {
 [SerializeField] WoolLosePresentation losePresentation; Color losePreviousBackdrop;
 void RestoreLosePresentation(){if(!losePresentation)return;bool was=losePresentation.gameObject.activeSelf;losePresentation.gameObject.SetActive(false);if(was)modal.GetComponent<Image>().color=losePreviousBackdrop;modalCard.gameObject.SetActive(true);}
 public void ShowLose(bool canContinue,Action resume,Action restart,Action goHome){
 RestoreLosePresentation();RestoreWinPresentation();RestoreUnlockPresentation();close.gameObject.SetActive(false);losePreviousBackdrop=modal.GetComponent<Image>().color;modal.GetComponent<Image>().color=Color.black;modalCard.gameObject.SetActive(false);if(rewardButton)rewardButton.gameObject.SetActive(false);modal.SetActive(true);modal.transform.SetAsLastSibling();losePresentation.gameObject.SetActive(true);losePresentation.Open(canContinue,resume,restart,goHome);
 }
 public void BindLoseCoins(Action action){losePresentation.resumeCoins=action;}
#if UNITY_EDITOR
 public void AuthorLosePresentation(Sprite backdrop,Sprite duck,Sprite frame,Sprite gold,Sprite blue){
 if(losePresentation)return;
 var root=Rect("Layered level failed",modal.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var p=root.gameObject.AddComponent<WoolLosePresentation>();losePresentation=p;
 var card=Panel("Purple panel frame",root,new Vector2(.5f,.5f),new Vector2(760,1320),Vector2.zero,Color.white);p.card=card;card.GetComponent<Image>().enabled=false;card.GetComponent<Image>().sprite=frame;card.GetComponent<Image>().type=Image.Type.Sliced;
 var bg=Panel("Separate castle background",card,new Vector2(.5f,.5f),new Vector2(680,760),new Vector2(0,190),Color.white);bg.GetComponent<Image>().sprite=backdrop;bg.SetParent(root,false);bg.SetAsFirstSibling();bg.anchorMin=bg.anchorMax=new Vector2(.5f,1);bg.pivot=new Vector2(.5f,1);bg.anchoredPosition=Vector2.zero;p.background=bg;
 var hero=Rect("Separate duck family foreground",card,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(610,580),new Vector2(0,110));p.hero=hero;var hi=hero.gameObject.AddComponent<Image>();hi.sprite=duck;hi.preserveAspect=true;hi.raycastTarget=false;
 var banner=Panel("Purple title banner",card,new Vector2(.5f,.5f),new Vector2(710,150),new Vector2(0,585),new Color(.6f,.39f,.86f));banner.gameObject.SetActive(false);banner.GetComponent<Image>().sprite=frame;banner.GetComponent<Image>().type=Image.Type.Sliced;
 var title=Label("Editable failure title",card,"Level Failed!",66,new Vector2(650,110),new Vector2(0,585),Color.white);title.fontSharedMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/ProfileEditor/TitleText.mat");
 p.continueButton=ButtonAt("Continue with video",card,"Continue",new Vector2(590,125),new Vector2(0,-300),null);
 p.retry=ButtonAt("Retry level",card,"Retry",new Vector2(560,115),new Vector2(0,-445),null);
 p.home=ButtonAt("Return Home",card,"Home",new Vector2(560,115),new Vector2(0,-580),null);
 var buttons=new[]{p.continueButton,p.retry,p.home};
 for(int i=0;i<buttons.Length;i++){var b=buttons[i];b.GetComponent<Image>().sprite=i==0?gold:blue;b.GetComponent<Image>().type=Image.Type.Sliced;var text=b.GetComponentInChildren<TMP_Text>();text.fontSize=49;text.rectTransform.sizeDelta=new Vector2(480,95);text.color=Color.white;text.fontSharedMaterial=title.fontSharedMaterial;b.gameObject.AddComponent<WoolLoseButtonMotion>();var shine=b.gameObject.AddComponent<Coffee.UIEffects.UIShiny>();shine.width=.13f;shine.softness=.5f;shine.brightness=.55f;shine.gloss=.45f;shine.effectPlayer.play=true;shine.effectPlayer.loop=true;shine.effectPlayer.duration=.8f;shine.effectPlayer.loopDelay=2.5f;shine.effectPlayer.initialPlayDelay=i*.6f;shine.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;}
 var video=Rect("Reward video",p.continueButton.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(60,60),new Vector2(-205,0));video.gameObject.AddComponent<CanvasRenderer>();video.gameObject.AddComponent<WoolRewardVideoGraphic>().raycastTarget=false;
 p.tears=new Image[4];p.eyePositions=new[]{new Vector2(-19,145),new Vector2(66,104)};
 for(int i=0;i<4;i++){var tr=Rect("Animated tear "+i,hero,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(12,21),Vector2.zero);var im=tr.gameObject.AddComponent<Image>();im.sprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Defeat/WaterTear.asset");im.color=new Color(.3f,.8f,1);im.raycastTarget=false;p.tears[i]=im;}
 root.gameObject.SetActive(false);UnityEditor.EditorUtility.SetDirty(p);UnityEditor.EditorUtility.SetDirty(this);
 }
#endif
}}
