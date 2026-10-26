#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame.Editor {
public static class WoolMenuPolishAuthoring {
 static TMP_FontAsset font;static Material textMat;static Sprite coin,green,purple,blue,plate;static Color Brown=new Color(.29f,.12f,.045f);
 static RectTransform Rect(string n,Transform p,Vector2 size,Vector2 pos){var r=new GameObject(n,typeof(RectTransform),typeof(CanvasRenderer)).GetComponent<RectTransform>();r.SetParent(p,false);r.gameObject.layer=p.gameObject.layer;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=pos;return r;}
 static TMP_Text Text(string n,Transform p,string value,float fs,Vector2 size,Vector2 pos,Color color){var r=Rect(n,p,size,pos);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSharedMaterial=textMat;t.text=value;t.fontSize=fs;t.enableAutoSizing=true;t.fontSizeMax=fs;t.fontSizeMin=fs*.75f;t.color=color;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 static Image Pic(string n,Transform p,Sprite sprite,Vector2 size,Vector2 pos){var r=Rect(n,p,size,pos);var im=r.gameObject.AddComponent<Image>();im.sprite=sprite;im.preserveAspect=true;im.raycastTarget=false;return im;}
 static RectTransform Surface(string n,Transform p,Vector2 size,Vector2 pos,WoolSpinSurface.Kind kind){var r=Rect(n,p,size,pos);var g=r.gameObject.AddComponent<WoolSpinSurface>();g.kind=kind;g.raycastTarget=false;return r;}
 static Button Button(string n,Transform p,string title,Sprite sprite,Vector2 size,Vector2 pos,float fs=36){var im=Pic(n,p,sprite,size,pos);im.preserveAspect=false;im.raycastTarget=true;var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;var colors=b.colors;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.82f,.82f,.82f);colors.disabledColor=new Color(.7f,.7f,.7f);b.colors=colors;Text("Label",b.transform,title,fs,size-new Vector2(28,12),Vector2.zero,Color.white);return b;}
 static void Symbol(Transform parent,WoolMenuSymbol.Kind kind,Vector2 size,Vector2 pos){var r=Rect(kind+" icon",parent,size,pos);var g=r.gameObject.AddComponent<WoolMenuSymbol>();g.kind=kind;g.color=new Color(1,.92f,.61f);g.raycastTarget=false;}
 static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
 [MenuItem("Wool/Polish menus and map")]
 public static void Apply(){
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
  var hud=UnityEngine.Object.FindFirstObjectByType<WoolHud>(FindObjectsInactive.Include);var win=UnityEngine.Object.FindFirstObjectByType<WoolWinPresentation>(FindObjectsInactive.Include);if(!hud||!win)throw new InvalidOperationException("Missing HUD.");
  font=win.collectLabel.font;textMat=win.collectLabel.fontSharedMaterial;coin=win.walletCoin.sprite;
  green=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Victory/VictoryButton2.png");purple=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Victory/VictoryButton0.png");blue=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Victory/VictoryButton1.png");plate=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/NamePlate.asset");
  AuthorUtility(hud,win);AuthorMap(hud);PolishShop(hud);PolishSpin();
  var guide=hud.transform.Find("Guided ability spotlight");if(guide)guide.gameObject.SetActive(false);
  foreach(var g in hud.GetComponentsInChildren<Graphic>(true))if((g is WoolVictoryGlow||g is WoolVictoryTopIcon||g is WoolMenuSymbol)&&!g.GetComponent<CanvasRenderer>())g.gameObject.AddComponent<CanvasRenderer>();
  EditorUtility.SetDirty(hud);EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(hud.gameObject.scene);
 }
 static void AuthorUtility(WoolHud hud,WoolWinPresentation win){
  var existing=hud.transform.Find("Settings and coins");WoolUtilityPanel u;
  if(existing){u=existing.GetComponent<WoolUtilityPanel>();}
  else{
   var root=Rect("Settings and coins",hud.transform,Vector2.zero,Vector2.zero);Stretch(root);var dim=root.gameObject.AddComponent<Image>();dim.color=new Color(.025f,.02f,.055f,.94f);dim.raycastTarget=true;u=root.gameObject.AddComponent<WoolUtilityPanel>();
   u.card=Surface("Gold framed card",root,new Vector2(860,1000),Vector2.zero,WoolSpinSurface.Kind.CreamCard);
   var heading=Surface("Amber title ribbon",u.card,new Vector2(780,150),new Vector2(0,420),WoolSpinSurface.Kind.Banner);u.title=Text("Editable title",heading,"Settings",66,new Vector2(610,110),Vector2.zero,Color.white);
   u.close=Button("Close",u.card,"",blue,new Vector2(88,88),new Vector2(367,455));var x=Rect("Close icon",u.close.transform,new Vector2(54,54),Vector2.zero);x.gameObject.AddComponent<WoolVictoryTopIcon>().raycastTarget=false;
   var settings=Rect("Audio controls",u.card,new Vector2(760,700),new Vector2(0,-30));u.settingsBody=settings.gameObject;
   Text("Settings subtitle",settings,"Make yourself at home",34,new Vector2(680,64),new Vector2(0,270),Brown);
   for(int i=0;i<2;i++){
    var row=Surface(i==0?"Music row":"Sound row",settings,new Vector2(725,185),new Vector2(0,115-i*215),WoolSpinSurface.Kind.CreamCard);
    var badge=Pic("Icon medallion",row,purple,new Vector2(112,112),new Vector2(-272,0));badge.preserveAspect=false;Symbol(badge.transform,i==0?WoolMenuSymbol.Kind.Music:WoolMenuSymbol.Kind.Sound,new Vector2(74,74),Vector2.zero);
    Text("Setting",row,i==0?"Music":"Sound effects",43,new Vector2(345,80),new Vector2(-35,0),Brown);
    var b=Button("Toggle",row,"ON",green,new Vector2(180,95),new Vector2(250,0),42);
    if(i==0){u.music=b;u.musicState=b.GetComponentInChildren<TMP_Text>();}else{u.sound=b;u.soundState=b.GetComponentInChildren<TMP_Text>();}
   }
   Text("Settings footer",settings,"Your choices are saved automatically",26,new Vector2(710,80),new Vector2(0,-290),new Color(.48f,.27f,.13f));
   var body=Rect("Coin offers",u.card,new Vector2(760,760),new Vector2(0,-40));u.coinsBody=body.gameObject;
   var wallet=Pic("Coin balance",body,plate,new Vector2(445,112),new Vector2(0,280));Pic("Duck coin",wallet.transform,coin,new Vector2(105,105),new Vector2(-155,0));u.balance=Text("Current coins",wallet.transform,"0",56,new Vector2(290,90),new Vector2(45,0),Brown);
   var gift=Surface("Daily coin gift",body,new Vector2(735,170),new Vector2(0,100),WoolSpinSurface.Kind.CreamCard);Pic("Daily duck coin",gift,coin,new Vector2(116,116),new Vector2(-273,0));Text("Daily gift title",gift,"Daily gift",41,new Vector2(320,62),new Vector2(-60,30),Brown);Text("Daily gift subtitle",gift,"50 coins · Every day",26,new Vector2(330,54),new Vector2(-60,-30),Brown);u.daily=Button("Claim daily gift",gift,"CLAIM +50",green,new Vector2(240,94),new Vector2(225,0),33);u.dailyState=u.daily.GetComponentInChildren<TMP_Text>();
   var video=Surface("Optional video reward",body,new Vector2(735,170),new Vector2(0,-100),WoolSpinSurface.Kind.CreamCard);var v=Rect("Video icon",video,new Vector2(100,100),new Vector2(-273,0));v.gameObject.AddComponent<WoolRewardVideoGraphic>().raycastTarget=false;Text("Video title",video,"Extra coins",41,new Vector2(320,62),new Vector2(-60,30),Brown);Text("Video subtitle",video,"Watch a video · +50",26,new Vector2(330,54),new Vector2(-60,-30),Brown);u.video=Button("Watch rewarded video",video,"WATCH",purple,new Vector2(240,94),new Vector2(225,0),36);
   u.shop=Button("Browse coin packs",body,"Visit Coin Shop",green,new Vector2(580,118),new Vector2(0,-300),46);
  }
  u.greenButton=green;u.greyButton=win.collectedButton;u.gameObject.SetActive(false);var so=new SerializedObject(hud);so.FindProperty("utilityPanel").objectReferenceValue=u;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(u);
 }
 static void AuthorMap(WoolHud hud){
  var map=UnityEngine.Object.FindFirstObjectByType<WoolSkywayMap>(FindObjectsInactive.Include);if(!map)return;
  var header=map.GetComponent<WoolMapTopBar>();if(!header){header=map.gameObject.AddComponent<WoolMapTopBar>();header.owner=hud;header.map=map;header.bar=Surface("Map wallet and settings",map.transform,new Vector2(900,116),Vector2.zero,WoolSpinSurface.Kind.CreamCard);header.bar.anchorMin=header.bar.anchorMax=new Vector2(.5f,1);header.bar.pivot=new Vector2(.5f,1);
   var wallet=Button("Map coin wallet",header.bar,"",green,new Vector2(345,88),new Vector2(-245,0));wallet.GetComponent<Image>().sprite=plate;wallet.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);Pic("Duck coin",wallet.transform,coin,new Vector2(78,78),new Vector2(-125,0));header.balance=Text("Coin balance",wallet.transform,"0",44,new Vector2(205,70),new Vector2(-5,0),Brown);Text("Add coins",wallet.transform,"+",50,new Vector2(55,65),new Vector2(135,0),new Color(.16f,.55f,.12f));header.coins=wallet;
   header.current=Button("Current level",header.bar,"LEVEL 1",blue,new Vector2(300,87),new Vector2(100,0),39);header.level=header.current.GetComponentInChildren<TMP_Text>();
   header.settings=Button("Map settings",header.bar,"",blue,new Vector2(90,88),new Vector2(365,0));Symbol(header.settings.transform,WoolMenuSymbol.Kind.Gear,new Vector2(62,62),Vector2.zero);
  }
  map.pageLabel.fontSizeMax=31;map.pageLabel.fontSizeMin=23;map.pageLabel.color=Brown;map.status.fontSizeMax=19;map.status.fontSizeMin=14;map.status.color=new Color(.43f,.24f,.10f);
  var caption=(RectTransform)map.pageLabel.transform.parent;var bg=caption.GetComponent<Image>();if(bg){bg.sprite=plate;bg.color=new Color(1,1,1,.95f);}
  foreach(var arrow in new[]{map.previous,map.next}){arrow.GetComponent<Image>().sprite=blue;arrow.GetComponent<Image>().color=Color.white;var t=arrow.GetComponentInChildren<TMP_Text>();if(t)t.color=Color.white;}
  EditorUtility.SetDirty(header);EditorUtility.SetDirty(map);
 }
 static void PolishShop(WoolHud hud){
  var view=UnityEngine.Object.FindFirstObjectByType<WoolShopView>(FindObjectsInactive.Include);if(!view)return;view.designHeight=2810;
  var heading=view.masthead.Find("Editable shop heading").GetComponent<TMP_Text>();heading.text="Shop";heading.color=new Color(1,.97f,.7f);heading.fontSizeMax=82;
  var wallet=view.masthead.Find("Duck coin wallet");view.balance.color=Brown;view.balance.fontSizeMax=39;var wb=wallet.GetComponent<Button>();if(!wb){wb=wallet.gameObject.AddComponent<Button>();wallet.GetComponent<Image>().raycastTarget=true;UnityEditor.Events.UnityEventTools.AddPersistentListener(wb.onClick,hud.ShowCoinOptions);}
  if(!wallet.Find("Coin options plus"))Text("Coin options plus",wallet,"+",42,new Vector2(46,60),new Vector2(106,0),new Color(.10f,.5f,.05f));view.balance.rectTransform.anchoredPosition=new Vector2(3,0);view.balance.rectTransform.sizeDelta=new Vector2(165,64);
  if(!view.design.Find("Bundles heading"))Text("Bundles heading",view.design,"POWER BUNDLES",33,new Vector2(840,56),new Vector2(0,-382),new Color(1,.91f,.61f));
  var bh=view.design.Find("Bundles heading").GetComponent<RectTransform>();bh.anchorMin=bh.anchorMax=new Vector2(.5f,1);bh.anchoredPosition=new Vector2(0,-382);
  for(int i=1;i<=3;i++){var offer=view.offers[i];var row=(RectTransform)offer.purchase.transform.parent;row.anchoredPosition=new Vector2(0,-610-(i-1)*400);offer.title.fontSizeMax=48;offer.title.color=Color.white;offer.title.rectTransform.sizeDelta=new Vector2(590,68);offer.title.rectTransform.anchoredPosition=new Vector2(-130,137);foreach(var count in offer.counts)if(count){count.fontSizeMax=30;count.color=Brown;}offer.Refresh();}
  if(!view.design.Find("Coin packs heading"))Text("Coin packs heading",view.design,"COIN PACKS",33,new Vector2(840,56),new Vector2(0,-1640),new Color(1,.91f,.61f));var ch=view.design.Find("Coin packs heading").GetComponent<RectTransform>();ch.anchorMin=ch.anchorMax=new Vector2(.5f,1);ch.anchoredPosition=new Vector2(0,-1640);
  for(int i=0;i<6;i++){var offer=view.offers[i+4];var row=(RectTransform)offer.purchase.transform.parent;row.anchoredPosition=new Vector2((i%3-1)*306,-1840-(i/3)*320);offer.coinAmount.fontSizeMax=43;offer.coinAmount.color=Brown;offer.priceText.fontSizeMax=34;}
  var gift=(RectTransform)view.dailyClaim.transform.parent;gift.anchoredPosition=new Vector2(0,-2530);gift.Find("Daily reward rule").GetComponent<TMP_Text>().text="A fresh gift every day";view.claimText.fontSizeMax=36;
  // These remain the user's editable catalogue values; no prices or quantities are rewritten.
  EditorUtility.SetDirty(view);view.Fit(true);
 }
 static void PolishSpin(){
  var spin=UnityEngine.Object.FindFirstObjectByType<WoolLuckySpin>(FindObjectsInactive.Include);if(!spin)return;spin.dimmer.color=new Color(.025f,.02f,.065f,.98f);
  if(!spin.card.Find("Wheel golden aura")){var glow=Rect("Wheel golden aura",spin.card,new Vector2(1100,1100),new Vector2(0,220));glow.gameObject.AddComponent<WoolVictoryGlow>().raycastTarget=false;glow.SetAsFirstSibling();}
  var hub=spin.wheel.Find("Original flower hub");if(hub){var image=hub.GetComponent<Image>();image.sprite=coin;image.preserveAspect=true;image.useSpriteMesh=false;}
  for(int i=0;i<8;i++){var t=spin.wheel.Find("Prize label "+i).GetComponent<TMP_Text>();t.color=i==0||i%2==1?Color.white:Brown;t.fontSizeMax=i==0||i==4||i==5?31:45;}
  var title=spin.card.Find("Title").GetComponent<TMP_Text>();title.text="Lucky Spin";title.color=new Color(1,.97f,.8f);
  spin.availableButton=green;spin.unavailableButton=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Victory/VictoryButtonCollected.png");spin.spin.GetComponent<Image>().sprite=green;spin.buttonLabel.fontSizeMax=66;spin.buttonLabel.fontSizeMin=40;
  spin.guaranteeLabel.color=new Color(.49f,.17f,.65f);spin.resultLabel.color=new Color(1,.91f,.60f);spin.progressLabel.color=new Color(1,.96f,.86f);
  EditorUtility.SetDirty(spin);spin.Fit();
 }
}}
#endif
