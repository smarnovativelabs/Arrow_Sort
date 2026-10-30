using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed class WoolWinPresentation:MonoBehaviour {
 public RectTransform card,wallet,duck,rewardOrigin;
 public RectTransform rewardButtons,glow;
 public Image walletCoin;public Image[] flyingCoins;public Sprite[] danceFrames;
 public TMP_Text walletLabel,rewardLabel,collectLabel,bonusLabel,collectAmount,bonusAmount;
 public Button collect,bonus,home,close;public WoolWinConfetti confetti;
 public Sprite collectedButton;
 [Tooltip("Seconds per joyful duck pose")] public float danceFrameSeconds=.18f;
 Vector2 lastSize;Rect lastSafe;float clock;int displayBalance;bool busy,allowDouble,collecting;
 Action collectAction,bonusAction,homeAction;Image duckImage;Sprite bonusSkin,collectSkin;
 public bool Busy=>busy;
 public int DisplayBalance=>displayBalance;
 public void Open(int earned,int balance,bool canDouble,Action onCollect,Action onBonus,Action onHome){
  StopAllCoroutines();busy=collecting=false;allowDouble=canDouble;clock=0;lastSize=Vector2.zero;
  collectAction=onCollect;bonusAction=onBonus;homeAction=onHome;
  if(!duckImage)duckImage=duck.GetComponent<Image>();
  if(!bonusSkin)bonusSkin=bonus.GetComponent<Image>().sprite;if(!collectSkin)collectSkin=collect.GetComponent<Image>().sprite;
  bonus.GetComponent<Image>().sprite=canDouble?bonusSkin:collectedButton;collect.GetComponent<Image>().sprite=collectSkin;
  // The save already contains the base reward; animate from the pre-reward balance.
  displayBalance=Mathf.Max(0,balance-Mathf.Max(0,earned));walletLabel.text=WoolCoinText.Format(displayBalance);
  rewardLabel.text="+"+Mathf.Max(0,earned);
  collectLabel.text="Continue";bonusLabel.text=canDouble?"2x coins":"Collected";
  var mapCaption=home.GetComponentInChildren<TMP_Text>();if(mapCaption)mapCaption.text="Map";
  if(collectAmount)collectAmount.text="+"+Mathf.Max(0,earned);
  if(bonusAmount)bonusAmount.text=canDouble?"+"+(earned+50):"";
  collect.onClick.RemoveAllListeners();collect.onClick.AddListener(()=>Begin(collectAction));
  bonus.onClick.RemoveAllListeners();bonus.onClick.AddListener(()=>{if(allowDouble)Begin(bonusAction);});
  home.onClick.RemoveAllListeners();home.onClick.AddListener(()=>Begin(homeAction));
  if(close){close.onClick.RemoveAllListeners();close.onClick.AddListener(()=>Begin(collectAction));}
  LayoutRewardButton(collect,collectLabel,collectAmount,earned>0,false);
  LayoutRewardButton(bonus,bonusLabel,bonusAmount,canDouble,canDouble);
  SetButtons(true);foreach(var coin in flyingCoins)if(coin)coin.gameObject.SetActive(false);
  Fit();if(confetti)confetti.Play();
 }
 void LayoutRewardButton(Button button,TMP_Text label,TMP_Text amount,bool showAmount,bool showVideo){
  var video=button.transform.Find("Reward video icon");if(video){video.gameObject.SetActive(showVideo);var vr=(RectTransform)video;vr.anchoredPosition=new Vector2(-120,-34);vr.sizeDelta=new Vector2(46,44);}
  var coin=button.transform.Find("Reward coin");if(coin)coin.gameObject.SetActive(showAmount);
  if(amount)amount.gameObject.SetActive(showAmount);
  var bounds=((RectTransform)button.transform).rect;
  var rt=label.rectTransform;rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);
  rt.localScale=Vector3.one;rt.anchoredPosition=new Vector2(0,showAmount?26:0);
  rt.sizeDelta=new Vector2(bounds.width-80,showAmount?48:70);
  label.enableAutoSizing=true;label.fontSizeMin=24;label.fontSizeMax=40;
  label.textWrappingMode=TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Truncate;
  label.alignment=TextAlignmentOptions.Center;label.margin=new Vector4(4,2,4,2);
  if(amount){var ar=amount.rectTransform;ar.anchorMin=ar.anchorMax=ar.pivot=new Vector2(.5f,.5f);
   ar.localScale=Vector3.one;ar.anchoredPosition=new Vector2(showVideo?62:34,-28);
   ar.sizeDelta=new Vector2(140,46);amount.enableAutoSizing=true;amount.fontSizeMin=24;amount.fontSizeMax=36;
   amount.textWrappingMode=TextWrappingModes.NoWrap;amount.overflowMode=TextOverflowModes.Truncate;amount.alignment=TextAlignmentOptions.Center;}
  if(coin){var cr=(RectTransform)coin;cr.anchoredPosition=new Vector2(showVideo?-38:-72,-28);cr.sizeDelta=new Vector2(46,46);}
  if(video){var vr=(RectTransform)video;vr.anchoredPosition=new Vector2(-104,-28);vr.sizeDelta=new Vector2(42,40);}
 }
 void Begin(Action action){if(busy||action==null)return;busy=true;SetButtons(false);action();}
 void SetButtons(bool enabled){collect.interactable=home.interactable=enabled;if(close)close.interactable=enabled;bonus.interactable=enabled&&allowDouble;}
 void OnDisable(){StopAllCoroutines();busy=collecting=false;if(flyingCoins!=null)foreach(var c in flyingCoins)if(c)c.gameObject.SetActive(false);if(walletCoin)walletCoin.rectTransform.localScale=Vector3.one;}
 public void Fit(){var root=transform as RectTransform;if(!root)return;var size=root.rect.size;var safe=WoolHud.CurrentSafeArea;
  if(lastSize==size&&lastSafe==safe)return;lastSize=size;lastSafe=safe;
  if(Screen.width<=0||Screen.height<=0)return;
  float top=(Screen.height-safe.yMax)/Screen.height*size.y,bottom=safe.yMin/Screen.height*size.y;
  float left=safe.xMin/Screen.width*size.x,right=(Screen.width-safe.xMax)/Screen.width*size.x;
  float safeWidth=size.x-left-right,center=(left-right)*.5f;
  float safeHeight=size.y-top-bottom;
  float buttonScale=Mathf.Max(.1f,Mathf.Min(1.15f,(safeWidth-48)/820f,safeHeight*.17f/160f));
  float topScale=Mathf.Max(.1f,Mathf.Min(1,safeWidth/650f,safeHeight*.12f/112f));
  float footerBottom=bottom+32;
  if(rewardButtons){rewardButtons.localScale=Vector3.one*buttonScale;rewardButtons.anchoredPosition=new Vector2(center,footerBottom+80*buttonScale);}
  float contentBottom=-size.y*.5f+footerBottom+160*buttonScale+32;
  float contentTop=size.y*.5f-top-24-112*topScale-24;
  // Keep all celebration elements in one fixed frame, between header and actions.
  float scale=Mathf.Max(.1f,Mathf.Min((safeWidth-48)/970f,(contentTop-contentBottom)/1000f));
  card.localScale=Vector3.one*scale;card.anchoredPosition=new Vector2(center,(contentBottom+contentTop)*.5f);
  var banner=card.Find("Victory banner") as RectTransform;if(banner){banner.anchoredPosition=new Vector2(0,350);banner.sizeDelta=new Vector2(760,300);}
  if(duck)duck.sizeDelta=new Vector2(430,430);
  var rewardRow=(RectTransform)rewardLabel.transform.parent;rewardRow.anchoredPosition=new Vector2(0,-330);
  var caption=card.Find("Victory caption") as RectTransform;if(caption)caption.anchoredPosition=new Vector2(0,-430);
  wallet.localScale=Vector3.one*topScale;wallet.anchoredPosition=new Vector2(center,-top-24*topScale);
  var h=(RectTransform)home.transform;h.localScale=Vector3.one*topScale;h.anchoredPosition=new Vector2(left+22,-top-22);
  if(close){var c=(RectTransform)close.transform;c.localScale=Vector3.one*topScale;c.anchoredPosition=new Vector2(-right-22,-top-22);}
 }
 void Update(){
  Fit();clock+=Mathf.Min(Time.unscaledDeltaTime,.1f);
  if(duck){
   if(duckImage&&danceFrames!=null&&danceFrames.Length>0)duckImage.sprite=danceFrames[Mathf.FloorToInt(clock/Mathf.Max(.08f,danceFrameSeconds))%danceFrames.Length];
   float beat=Mathf.Sin(clock*Mathf.PI*2/1.08f);
   duck.anchoredPosition=new Vector2(beat*9,-48+Mathf.Abs(beat)*12);duck.localRotation=Quaternion.Euler(0,0,beat*3);
   duck.localScale=new Vector3(1+beat*.015f,1-beat*.015f,1);
  }
  if(glow)glow.localRotation=Quaternion.Euler(0,0,clock*5);
 }
 public void Collect(int amount,int balance,Action after,bool doubled=false){
  if(!gameObject.activeInHierarchy||collecting)return;collecting=true;busy=true;allowDouble=false;SetButtons(false);
  if(amount>0){var audio=FindFirstObjectByType<WoolGameAudio>();if(audio)audio.Play(WoolGameAudio.Cue.Coin);}
  var source=(RectTransform)(doubled?bonus:collect).transform;
  bonusLabel.text="Collected";if(bonusAmount)bonusAmount.text="";LayoutRewardButton(bonus,bonusLabel,bonusAmount,false,false);
  if(collectedButton)bonus.GetComponent<Image>().sprite=collectedButton;
  if(amount>0)rewardLabel.text="+"+WoolCoinText.Format(amount);
  StopAllCoroutines();StartCoroutine(Fly(amount,balance,source,after));
 }
 IEnumerator Fly(int amount,int balance,RectTransform source,Action after){
  var root=(RectTransform)transform;int start=displayBalance,count=Mathf.Min(3,flyingCoins.Length),arrived=0;
  if(amount>0&&count>0){
   for(float elapsed=0;elapsed<1.3f;elapsed+=Mathf.Min(Time.unscaledDeltaTime,.05f)){
    Vector3 origin=root.InverseTransformPoint(source.TransformPoint(source.rect.center)),target=root.InverseTransformPoint(walletCoin.transform.position);
    for(int i=0;i<count;i++){
     float t=Mathf.Clamp01((elapsed-i*.14f)/.88f);var im=flyingCoins[i];im.gameObject.SetActive(t>0&&t<1);if(t>=1&&arrived<i+1){arrived=i+1;displayBalance=start+Mathf.RoundToInt((balance-start)*(arrived/(float)count));walletLabel.text=WoolCoinText.Format(displayBalance);}
     if(t<=0||t>=1)continue;float u=t*t*(3-2*t);var bend=(origin+target)*.5f+new Vector3((i-1)*120,150);
     im.rectTransform.anchoredPosition=Vector3.Lerp(Vector3.Lerp(origin,bend,u),Vector3.Lerp(bend,target,u),u);
     im.rectTransform.localScale=Vector3.one*Mathf.Lerp(1.15f,.6f,t);im.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI*3)*18);
    }
    walletCoin.rectTransform.localScale=Vector3.one*(1+(arrived>0?.12f*Mathf.Abs(Mathf.Sin(elapsed*26)):0));yield return null;
   }
  }
  foreach(var im in flyingCoins)if(im)im.gameObject.SetActive(false);walletCoin.rectTransform.localScale=Vector3.one;
  displayBalance=balance;walletLabel.text=WoolCoinText.Format(balance);collectLabel.text="Collected";if(collectAmount)collectAmount.text="";
  if(collectedButton)collect.GetComponent<Image>().sprite=collectedButton;
  yield return new WaitForSecondsRealtime(.35f);
  // Keep actions locked through navigation; a fast second tap cannot repeat the payout.
  after?.Invoke();
 }
}
public sealed partial class WoolHud {
 [SerializeField] WoolWinPresentation winPresentation;Color winPreviousBackdrop;
 void RestoreWinPresentation(){if(!winPresentation)return;bool was=winPresentation.gameObject.activeSelf;winPresentation.gameObject.SetActive(false);if(was)modal.GetComponent<Image>().color=winPreviousBackdrop;modalCard.gameObject.SetActive(true);}
 public void ShowLevelComplete(int earned,int balance,bool canDouble,Action collected,Action doubled,Action homeAction){
  HideAbilityGuide();HideAbilityEffect();Tutorial(false);
  RestoreLosePresentation();RestoreWinPresentation();RestoreUnlockPresentation();winPreviousBackdrop=modal.GetComponent<Image>().color;modal.GetComponent<Image>().color=new Color(0,0,0,.86f);
  modalCard.gameObject.SetActive(false);if(rewardButton)rewardButton.gameObject.SetActive(false);modal.SetActive(true);modal.transform.SetAsLastSibling();winPresentation.gameObject.SetActive(true);winPresentation.Open(earned,balance,canDouble,collected,doubled,homeAction);
 }
 public void FlyWinCoins(int amount,int balance,Action after,bool doubled=false){winPresentation.Collect(amount,balance,after,doubled);}
#if UNITY_EDITOR

 public void AuthorReferenceWinLayout(Sprite artwork){
 var p=winPresentation;if(!p||!artwork)throw new InvalidOperationException("Missing win presentation or artwork");
 var root=(RectTransform)p.transform;var card=p.card;
 card.sizeDelta=new Vector2(820,1150);card.GetComponent<Image>().enabled=false;
 foreach(Transform child in card){if(child.name=="Victory banner"||child.name=="Editable victory title"||child.name=="Editable celebration subtitle"||child.name=="Icon 9")child.gameObject.SetActive(false);}
 p.duck.GetComponent<Image>().sprite=artwork;p.duck.sizeDelta=new Vector2(820,880);p.duck.anchoredPosition=new Vector2(0,155);
 var row=(RectTransform)p.rewardLabel.transform.parent;row.sizeDelta=new Vector2(640,118);row.anchoredPosition=new Vector2(0,-325);
 p.rewardOrigin.anchoredPosition=new Vector2(-155,0);p.rewardOrigin.sizeDelta=new Vector2(105,105);
 p.rewardLabel.rectTransform.anchoredPosition=new Vector2(65,0);p.rewardLabel.rectTransform.sizeDelta=new Vector2(350,100);p.rewardLabel.fontSize=68;p.rewardLabel.fontSizeMax=68;p.rewardLabel.fontSizeMin=48;
 var buttons=new[]{p.bonus,p.home,p.collect};var cols=new[]{new Color(.68f,.22f,1),new Color(.12f,.65f,1),new Color(.2f,.9f,.24f)};
 for(int i=0;i<3;i++){var b=buttons[i];var rt=(RectTransform)b.transform;rt.sizeDelta=new Vector2(248,132);rt.anchoredPosition=new Vector2((i-1)*266,-490);b.GetComponent<Image>().sprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Defeat/GoldButton.asset");b.GetComponent<Image>().color=cols[i];
 var label=b.GetComponentInChildren<TMP_Text>(true);label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);label.rectTransform.anchoredPosition=Vector2.zero;label.rectTransform.sizeDelta=new Vector2(220,105);label.fontSize=40;label.fontSizeMin=26;label.fontSizeMax=40;label.enableAutoSizing=true;label.color=Color.white;
 foreach(Transform ch in b.transform){if(ch.GetComponent<Image>()||ch.name=="Reward video icon")ch.gameObject.SetActive(false);}
 }
 p.collectLabel.text="Continue";p.bonusLabel.text="2x Reward";p.home.GetComponentInChildren<TMP_Text>().text="Home";
 p.wallet.pivot=Vector2.one;p.wallet.anchorMin=p.wallet.anchorMax=Vector2.one;p.wallet.sizeDelta=new Vector2(330,90);
 p.walletCoin.rectTransform.anchorMin=p.walletCoin.rectTransform.anchorMax=new Vector2(.5f,.5f);p.walletCoin.rectTransform.anchoredPosition=new Vector2(-110,0);p.walletCoin.rectTransform.sizeDelta=new Vector2(88,88);
 p.walletLabel.rectTransform.anchoredPosition=new Vector2(40,0);p.walletLabel.rectTransform.sizeDelta=new Vector2(210,74);
 var bg=root.Find("Victory full screen background") as RectTransform;
 if(!bg){bg=new GameObject("Victory full screen background",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)).GetComponent<RectTransform>();bg.SetParent(root,false);}
 bg.anchorMin=Vector2.zero;bg.anchorMax=Vector2.one;bg.offsetMin=bg.offsetMax=Vector2.zero;bg.SetAsFirstSibling();
 const string path="Assets/WoolGame/Designer/UI/Victory/WarmVictoryBackdrop.png";
 var tex=new Texture2D(128,256,TextureFormat.RGBA32,false);
 for(int y=0;y<256;y++)for(int x=0;x<128;x++){float u=x/127f,v=y/255f;var c=Color.Lerp(new Color(.3f,.09f,.2f),new Color(.27f,.13f,.22f),v);float glow=Mathf.Exp(-((u-.5f)*(u-.5f)*5+(v-.4f)*(v-.4f)*8));c=Color.Lerp(c,new Color(1,.64f,.19f),glow*.94f);tex.SetPixel(x,y,c);}
 tex.Apply();System.IO.File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);UnityEditor.AssetDatabase.ImportAsset(path);
 var imp=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);imp.textureType=UnityEditor.TextureImporterType.Sprite;imp.spriteImportMode=UnityEditor.SpriteImportMode.Single;imp.SaveAndReimport();
 bg.GetComponent<Image>().sprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);bg.GetComponent<Image>().raycastTarget=false;
 AuthorVictoryButtonSkins();UnityEditor.EditorUtility.SetDirty(p);UnityEditor.EditorUtility.SetDirty(this);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
 }


 public void AuthorVictoryButtonSkins(){
 var p=winPresentation;var buttons=new[]{p.bonus,p.home,p.collect};
 var colors=new[]{WoolArt.Hex("AE28F5"),WoolArt.Hex("1797EF"),WoolArt.Hex("38C82A")};
 for(int i=0;i<3;i++){var tex=new Texture2D(256,132,TextureFormat.RGBA32,false);
 for(int y=0;y<132;y++)for(int x=0;x<256;x++){
 float dx=Mathf.Max(Mathf.Abs(x-127.5f)-96,0),dy=Mathf.Max(Mathf.Abs(y-65.5f)-34,0),d=Mathf.Sqrt(dx*dx+dy*dy)-26;
 float alpha=Mathf.Clamp01(-d);float v=y/131f;
 Color col=Color.Lerp(colors[i]*.48f,Color.Lerp(colors[i],Color.white,.2f),v);
 if(v>.58f)col=Color.Lerp(col,Color.white,.18f*(1-v)/.42f);
 if(d>-6)col=Color.Lerp(WoolArt.Hex("B45B13"),WoolArt.Hex("FFF28B"),v);
 if(d>-2)col=Color.Lerp(WoolArt.Hex("954014"),WoolArt.Hex("FFD95C"),v);
 col.a=alpha;tex.SetPixel(x,y,col);
 }tex.Apply();string path="Assets/WoolGame/Designer/UI/Victory/VictoryButton"+i+".png";System.IO.File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);UnityEditor.AssetDatabase.ImportAsset(path);
 var imp=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);imp.textureType=UnityEditor.TextureImporterType.Sprite;imp.spriteImportMode=UnityEditor.SpriteImportMode.Single;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.SaveAndReimport();
 var im=buttons[i].GetComponent<Image>();im.sprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);im.color=Color.white;im.type=Image.Type.Simple;
 var label=buttons[i].GetComponentInChildren<TMP_Text>(true);var mat=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/Victory/ButtonText.mat");if(!mat){mat=new Material(label.font.material);mat.SetFloat("_OutlineWidth",.15f);mat.SetColor("_OutlineColor",new Color32(76,35,11,255));UnityEditor.AssetDatabase.CreateAsset(mat,"Assets/WoolGame/Designer/UI/Victory/ButtonText.mat");}label.fontSharedMaterial=mat;label.fontSizeMax=38;label.fontSizeMin=25;
 }
 }

 public void AuthorWinPresentation(Sprite happyDuck,Sprite headerArt){
  if(winPresentation)return;
  var root=Rect("Level complete presentation",modal.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);winPresentation=root.gameObject.AddComponent<WoolWinPresentation>();var p=winPresentation;
  var frame=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/Card.asset");
  var green=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/SaveButton.asset");
  var yellow=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/Banner.asset");
  var plate=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/NamePlate.asset");
  var white=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/ProfileEditor/TitleText.mat");
  var card=Panel("Victory card",root,new Vector2(.5f,.5f),new Vector2(760,1000),Vector2.zero,Color.white);card.GetComponent<Image>().sprite=frame;card.GetComponent<Image>().raycastTarget=false;p.card=card;
  var header=Rect("Victory banner",card,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(760,160),new Vector2(0,470));var hi=header.gameObject.AddComponent<Image>();hi.sprite=headerArt;hi.preserveAspect=true;hi.raycastTarget=false;
  var title=Label("Editable victory title",card,"Level Complete!",62,new Vector2(650,95),new Vector2(0,470),Color.white);if(white)title.fontSharedMaterial=white;
  for(int i=0;i<8;i++){var c=Icon(card,9,new Vector2(84,84),new Vector2(-250+i*71,-85-Mathf.Abs(Mathf.Sin(i*2))*35));c.sprite=skin[5];}
  var hero=Rect("Happy dancing duck",card,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(530,470),new Vector2(0,150));var heroImage=hero.gameObject.AddComponent<Image>();heroImage.sprite=happyDuck;heroImage.preserveAspect=true;heroImage.raycastTarget=false;p.duck=hero;
  var row=Panel("Reward plate",card,new Vector2(.5f,.5f),new Vector2(630,140),new Vector2(0,-205),Color.white);row.GetComponent<Image>().sprite=plate;row.GetComponent<Image>().raycastTarget=false;
  p.rewardOrigin=Icon(row,9,new Vector2(100,100),new Vector2(-150,0)).rectTransform;p.rewardOrigin.GetComponent<Image>().sprite=skin[5];p.rewardLabel=Label("Earned coins",row,"+50",65,new Vector2(330,100),new Vector2(45,0),WoolArt.Hex("713415"));
  p.collect=ButtonAt("Collect level coins",card,"",new Vector2(315,125),new Vector2(-165,-348),null);p.collect.GetComponent<Image>().sprite=green;p.collect.GetComponent<Image>().type=Image.Type.Sliced;
  var cc=Icon(p.collect.transform,9,new Vector2(65,65),new Vector2(-100,0));cc.sprite=skin[5];p.collectLabel=p.collect.GetComponentInChildren<TMP_Text>();p.collectLabel.text="Collect";p.collectLabel.fontSize=39;p.collectLabel.rectTransform.sizeDelta=new Vector2(210,90);p.collectLabel.rectTransform.anchoredPosition=new Vector2(35,0);p.collectLabel.color=Color.white;if(white)p.collectLabel.fontSharedMaterial=white;
  p.bonus=ButtonAt("Double level reward",card,"",new Vector2(315,125),new Vector2(165,-348),null);p.bonus.GetComponent<Image>().sprite=yellow;p.bonus.GetComponent<Image>().type=Image.Type.Sliced;
  var video=Rect("Reward video icon",p.bonus.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(65,67),new Vector2(-100,0));video.gameObject.AddComponent<CanvasRenderer>();video.gameObject.AddComponent<WoolRewardVideoGraphic>().raycastTarget=false;
  p.bonusLabel=p.bonus.GetComponentInChildren<TMP_Text>();p.bonusLabel.text="2x\nReward";p.bonusLabel.fontSize=35;p.bonusLabel.rectTransform.sizeDelta=new Vector2(200,110);p.bonusLabel.rectTransform.anchoredPosition=new Vector2(35,0);p.bonusLabel.color=Color.white;if(white)p.bonusLabel.fontSharedMaterial=white;
  p.home=ButtonAt("Victory Home",card,"Home",new Vector2(360,92),new Vector2(0,-454),null);p.home.GetComponent<Image>().sprite=skin[2];p.home.GetComponent<Image>().type=Image.Type.Sliced;var homeText=p.home.GetComponentInChildren<TMP_Text>();homeText.fontSize=42;homeText.color=Color.white;if(white)homeText.fontSharedMaterial=white;
  var wr=Panel("Current coin balance",root,Vector2.one,new Vector2(300,82),new Vector2(-185,-70),Color.white);wr.pivot=Vector2.one;wr.anchoredPosition=new Vector2(-28,-28);wr.GetComponent<Image>().sprite=plate;wr.GetComponent<Image>().raycastTarget=false;p.wallet=wr;p.walletCoin=Icon(wr,9,new Vector2(90,90),new Vector2(-112,0));p.walletCoin.sprite=skin[5];p.walletLabel=Label("Current coins",wr,"0",46,new Vector2(210,65),new Vector2(25,0),WoolArt.Hex("713415"));
  var conf=Rect("Victory confetti particles",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);conf.gameObject.AddComponent<CanvasRenderer>();p.confetti=conf.gameObject.AddComponent<WoolWinConfetti>();p.confetti.raycastTarget=false;wr.SetAsLastSibling();
  p.flyingCoins=new Image[12];for(int i=0;i<p.flyingCoins.Length;i++){p.flyingCoins[i]=Icon(root,9,new Vector2(56,56),Vector2.zero);p.flyingCoins[i].name="Flying duck coin "+i;p.flyingCoins[i].sprite=skin[5];p.flyingCoins[i].gameObject.SetActive(false);}
  var bs=new[]{p.collect,p.bonus,p.home};for(int i=0;i<bs.Length;i++){var shine=bs[i].gameObject.AddComponent<Coffee.UIEffects.UIShiny>();shine.width=.12f;shine.softness=.55f;shine.brightness=.65f;shine.gloss=.55f;shine.rotation=25;shine.effectPlayer.play=true;shine.effectPlayer.loop=true;shine.effectPlayer.duration=.7f;shine.effectPlayer.loopDelay=2.2f;shine.effectPlayer.initialPlayDelay=.7f+i*.45f;shine.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;}
  foreach(var t in new[]{title,p.collectLabel,p.bonusLabel,homeText,p.rewardLabel,p.walletLabel}){t.enableAutoSizing=true;t.fontSizeMin=t.fontSize*.75f;t.fontSizeMax=t.fontSize;}
  root.gameObject.SetActive(false);UnityEditor.EditorUtility.SetDirty(p);UnityEditor.EditorUtility.SetDirty(this);
 }
#endif
}}

