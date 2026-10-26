using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed class WoolUnlockPresentation:MonoBehaviour {
 public RectTransform card,glow; public Image ability; public Image paymentIcon; public Sprite coinSprite; public GameObject videoIcon;
 public RectTransform[] buttons,shines;
 public Sprite[] abilityIcons;
 public TMP_Text heading,description,coinLabel,videoLabel; public TMP_Text walletLabel; public RectTransform wallet;
 [Header("Editable popup copy")]
 public string[] titles={"Unlock","Remove","Sort","Boost"};
 [TextArea] public string[] descriptions={"Unlock a new slot","Remove any box","Refresh the dragon body colors","Attack the dragon under fog"};
 public string freeCaption="Free";
 public float pulseAmount=.025f,glowSpeed=10;
 float clock;Vector2 lastSize; int lastBalance=-1;
 public void Configure(int kind,bool owned=false){RefreshBalance();heading.text=titles[kind];description.text=descriptions[kind];coinLabel.text=owned?"Use":WoolBoosters.Prices[kind].ToString();videoLabel.text=owned?"Cancel":freeCaption;if(paymentIcon)paymentIcon.sprite=owned?abilityIcons[kind]:coinSprite;if(videoIcon)videoIcon.SetActive(!owned);ability.sprite=abilityIcons[kind];ability.preserveAspect=true;lastSize=Vector2.zero;Fit();}
 void RefreshBalance(){int balance=WoolProgress.Coins;if(walletLabel&&(lastBalance!=balance||walletLabel.text.Length==0)){walletLabel.text=balance.ToString();lastBalance=balance;}}
 void OnEnable(){clock=0;lastSize=Vector2.zero;lastBalance=-1;RefreshBalance();Fit();}
 void OnDisable(){if(buttons!=null)foreach(var b in buttons)if(b)b.localScale=Vector3.one;}
 void Fit(){var p=transform.parent as RectTransform;if(!p||!card)return;var size=p.rect.size;if(size==lastSize)return;lastSize=size;float scale=Mathf.Min((size.x-64)/card.sizeDelta.x,(size.y-250)/card.sizeDelta.y);card.localScale=Vector3.one*scale;if(wallet){wallet.localScale=Vector3.one*scale;var area=WoolHud.CurrentSafeArea;float insetRight=Screen.width>0?(Screen.width-area.xMax)*size.x/Screen.width:0;float insetTop=Screen.height>0?(Screen.height-area.yMax)*size.y/Screen.height:0;wallet.anchoredPosition=new Vector2(size.x*.5f-insetRight-wallet.sizeDelta.x*.5f*scale-28,size.y*.5f-insetTop-wallet.sizeDelta.y*.5f*scale-28);}}
 void Update(){Fit();RefreshBalance();clock+=Time.unscaledDeltaTime;if(glow){glow.localRotation=Quaternion.Euler(0,0,clock*glowSpeed);glow.localScale=Vector3.one*(1+.04f*Mathf.Sin(clock*2));}for(int i=0;i<buttons.Length;i++){buttons[i].localScale=Vector3.one*(1+pulseAmount*(.5f+.5f*Mathf.Sin(clock*3+i*.6f)));}}
}
public sealed partial class WoolHud {
 [SerializeField] WoolUnlockPresentation unlockPresentation;Color unlockPreviousBackdrop;
 void RestoreUnlockPresentation(){if(!unlockPresentation)return;bool was=unlockPresentation.gameObject.activeSelf;unlockPresentation.gameObject.SetActive(false);if(was)modal.GetComponent<Image>().color=unlockPreviousBackdrop;modalCard.gameObject.SetActive(true);}
 void ApplyUnlockPresentation(string title,string first,string other){int k=Array.IndexOf(WoolBoosters.Names,title);if(title=="UNLOCK SLOT")k=0;if(!unlockPresentation||k<0)return;unlockPresentation.Configure(k,first.StartsWith("USE POWER"));unlockPreviousBackdrop=modal.GetComponent<Image>().color;modal.GetComponent<Image>().color=new Color32(0,0,0,250);modalCard.gameObject.SetActive(false);unlockPresentation.gameObject.SetActive(true);}
 void BindUnlockPresentation(){if(!unlockPresentation)return;foreach(var b in unlockPresentation.GetComponentsInChildren<Button>(true)){b.onClick.RemoveAllListeners();if(b.name=="Unlock with coins")b.onClick.AddListener(()=>primary.onClick.Invoke());else if(b.name=="Unlock with video")b.onClick.AddListener(()=>secondary.onClick.Invoke());else b.onClick.AddListener(()=>close.onClick.Invoke());}}
#if UNITY_EDITOR
 public void AuthorLayeredAbilities(Sprite frame,Sprite[] sprites) {
 var p=unlockPresentation;if(!p)throw new InvalidOperationException("Missing authored popup");
 while(p.transform.childCount>0)UnityEngine.Object.DestroyImmediate(p.transform.GetChild(0).gameObject);
 p.abilityIcons=sprites;
 var card=Panel("Ability card",p.transform,new Vector2(.5f,.5f),new Vector2(760,900),Vector2.zero,Color.white);card.GetComponent<Image>().sprite=frame;card.GetComponent<Image>().type=Image.Type.Sliced;card.GetComponent<Image>().raycastTarget=false;p.card=card;
 var ribbon=Panel("Title ribbon",card,new Vector2(.5f,.5f),new Vector2(540,115),new Vector2(0,379),Color.white);ribbon.GetComponent<Image>().sprite=skin[6];ribbon.GetComponent<Image>().type=Image.Type.Sliced;ribbon.GetComponent<Image>().raycastTarget=false;
 p.heading=Label("Editable title",ribbon,"Unlock",58,new Vector2(470,90),Vector2.zero,Color.white);
 var glow=Rect("Rotating glow BEHIND ability",card,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(530,530),new Vector2(0,75));glow.gameObject.AddComponent<CanvasRenderer>();glow.gameObject.AddComponent<WoolAbilityGlow>().raycastTarget=false;p.glow=glow;
 var icon=Rect("Transparent ability icon",card,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(365,365),new Vector2(0,75));p.ability=icon.gameObject.AddComponent<Image>();p.ability.raycastTarget=false;
 p.description=Label("Editable description",card,"",37,new Vector2(650,112),new Vector2(0,-173),WoolArt.Hex("713415"));p.description.enableAutoSizing=true;p.description.fontSizeMin=26;p.description.fontSizeMax=37;
 p.buttons=new RectTransform[2];p.shines=new RectTransform[2];
 for(int i=0;i<2;i++){
 var rt=Panel(i==0?"Unlock with coins":"Unlock with video",card,new Vector2(.5f,.5f),new Vector2(302,120),new Vector2(i==0?-161:161,-331),Color.white);
 var im=rt.GetComponent<Image>();im.sprite=skin[i==0?1:2];im.type=Image.Type.Sliced;
 var b=rt.gameObject.AddComponent<Button>();b.transition=Selectable.Transition.ColorTint;var colors=b.colors;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.87f,.87f,.87f);b.colors=colors;
 
 var shine=Rect("Button shine",rt,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(24,75),Vector2.zero);shine.localRotation=Quaternion.Euler(0,0,-22);var si=shine.gameObject.AddComponent<Image>();si.color=new Color(1,1,1,.12f);si.raycastTarget=false;p.shines[i]=shine;
 if(i==0){var coin=Icon(rt,9,new Vector2(65,65),new Vector2(-88,0));coin.sprite=skin[5];}else{var video=Label("Video symbol",rt,"AD",23,new Vector2(55,65),new Vector2(-91,0),Color.white);}
 var text=Label(i==0?"Editable coin price":"Editable video caption",rt,i==0?"800":"Free",46,new Vector2(175,82),new Vector2(34,0),Color.white);if(i==0)p.coinLabel=text;else p.videoLabel=text;p.buttons[i]=rt;
 }
 var x=ButtonAt("Dismiss unlock",card,"×",new Vector2(86,86),new Vector2(328,394),null);x.GetComponent<Image>().sprite=skin[3];x.GetComponent<Image>().type=Image.Type.Sliced;var xt=x.GetComponentInChildren<TMP_Text>();xt.color=Color.white;xt.fontSize=55;
 p.Configure(0);p.gameObject.SetActive(false);BindUnlockPresentation();UnityEditor.EditorUtility.SetDirty(p);UnityEditor.EditorUtility.SetDirty(this);
 }
#endif
}}