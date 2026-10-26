using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
[Serializable] public sealed class WoolShopOffer {
 public string bundleName,price; public int coins; public int[] abilities=new int[4];
 public TMP_Text title,coinAmount,priceText; public TMP_Text[] counts; public Button purchase;
 public void Refresh(){if(title)title.text=bundleName;if(coinAmount)coinAmount.text=coins.ToString("N0");if(priceText)priceText.text=price;if(counts!=null)for(int i=0;i<counts.Length;i++)if(counts[i])counts[i].text="x"+(i<abilities.Length?abilities[i]:0);}
}
public sealed class WoolShopView:MonoBehaviour {
 public WoolHud owner;public RectTransform masthead,viewport,content,design;public WoolMenuNavigation navigation;
 public ScrollRect scroll;public TMP_Text balance,claimText;public Button dailyClaim;public WoolShopOffer[] offers;
 public float designHeight=2810;public const float DesignHeight=2575;float refreshAt;Vector2 lastSize;Rect lastSafe;int lastBalance=-1;
 void OnEnable(){Bind();Fit(true);Refresh();}
 void OnDisable(){if(dailyClaim)dailyClaim.onClick.RemoveListener(Claim);}
 void Bind(){if(dailyClaim){dailyClaim.onClick.RemoveListener(Claim);dailyClaim.onClick.AddListener(Claim);}if(offers!=null)foreach(var offer in offers){offer.Refresh();offer.purchase.onClick.RemoveAllListeners();offer.purchase.onClick.AddListener(()=>owner.ShopPurchaseUnavailable());}}
 public void FocusBundles(){Fit(true);scroll.verticalNormalizedPosition=1;}
 public void FocusDailyGift(){Fit(true);scroll.verticalNormalizedPosition=0;}
 public void FocusCoinPacks(){Fit(true);Canvas.ForceUpdateCanvases();float y=1850*design.localScale.x;float travel=content.rect.height-viewport.rect.height;scroll.verticalNormalizedPosition=travel>0?Mathf.Clamp01(1-(y-80)/travel):1;}
 void Claim(){if(WoolProgress.ClaimShopDailyGift()){var audio=FindFirstObjectByType<WoolGameAudio>();if(audio)audio.Play(WoolGameAudio.Cue.Coin);owner.RefreshShopBalance();Refresh();}}
 public void Refresh(){int coins=WoolProgress.Coins;if(balance&&lastBalance!=coins){lastBalance=coins;balance.text=coins.ToString("N0");}if(dailyClaim){bool can=WoolProgress.CanClaimShopDailyGift;dailyClaim.interactable=can;claimText.text=can?"CLAIM FREE":"CLAIMED";}}
 void LateUpdate(){Fit(false);if(Time.unscaledTime>=refreshAt){refreshAt=Time.unscaledTime+1;Refresh();}}
 public void Fit(bool force){var root=(RectTransform)transform;if(!viewport||!design||root.rect.width<=0)return;var size=root.rect.size;var safe=Screen.safeArea;if(!force&&size==lastSize&&safe==lastSafe)return;lastSize=size;lastSafe=safe;
 // Only interactive content follows the safe area; decorative art covers the device.
 var bg=transform.Find("Premium shop backdrop") as RectTransform;if(bg){var fitter=bg.GetComponent<AspectRatioFitter>();if(fitter)fitter.enabled=false;var im=bg.GetComponent<Image>();if(im&&im.sprite)WoolCanvasArt.Cover(bg,im.sprite.rect.width/im.sprite.rect.height);}
 float top=0,bottom=0;
 float scale=Mathf.Min((size.x-40)/900,1.4f);masthead.localScale=Vector3.one*scale;masthead.anchoredPosition=new Vector2(0,-top);
 float nav=navigation?navigation.Height+40:170;viewport.offsetMax=new Vector2(-16,-top-285*scale);viewport.offsetMin=new Vector2(16,Mathf.Max(nav,bottom+24));
 content.localScale=Vector3.one;content.sizeDelta=new Vector2(0,designHeight*scale);design.localScale=Vector3.one*scale;design.anchoredPosition=Vector2.zero;
 }
#if UNITY_EDITOR
 void OnValidate(){if(offers!=null)foreach(var offer in offers)if(offer!=null)offer.Refresh();}
#endif
}
public static partial class WoolProgress {
 public static bool CanClaimShopDailyGift{get{EnsureLoaded();return string.CompareOrdinal(data.lastShopGiftDay,TaskDay)<0;}}
 public static bool ClaimShopDailyGift(){EnsureLoaded();if(!CanClaimShopDailyGift||data.coins>int.MaxValue-50)return false;data.lastShopGiftDay=TaskDay;RecordDailyCoins(50);data.coins+=50;data.collectedCoins+=50;Save();return true;}
}
public sealed partial class WoolHud {
 public void ShopPurchaseUnavailable(){Show("STORE UNAVAILABLE","Purchases are currently unavailable.\nNo payment has been taken.","BACK TO SHOP",Hide);}
 public void RefreshShopBalance(){var value=WoolProgress.Coins.ToString();if(homeCoins)homeCoins.text=value;if(shopCoins)shopCoins.text=value;}
#if UNITY_EDITOR

 public void AuthorPremiumShop(Sprite[] art,Sprite backdrop){
 var old=shopPage.GetComponent<WoolShopView>();var previous=old?old.offers:null;
 AuthorReferenceShop(new[]{art[0],art[2],art[3],art[4]});
 var v=shopPage.GetComponent<WoolShopView>();
 if(previous!=null)for(int i=0;i<Mathf.Min(previous.Length,v.offers.Length);i++){v.offers[i].bundleName=previous[i].bundleName;v.offers[i].coins=previous[i].coins;v.offers[i].price=previous[i].price;v.offers[i].abilities=(int[])previous[i].abilities.Clone();}
 var bg=shopPage.Find("Premium shop backdrop") as RectTransform;
 if(!bg){bg=Rect("Premium shop backdrop",shopPage,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);bg.gameObject.AddComponent<Image>();}
 bg.anchorMin=Vector2.zero;bg.anchorMax=Vector2.one;bg.offsetMin=bg.offsetMax=Vector2.zero;bg.SetAsFirstSibling();
 var bi=bg.GetComponent<Image>();bi.sprite=backdrop;bi.raycastTarget=false;
 var fit=bg.GetComponent<AspectRatioFitter>();if(!fit)fit=bg.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=backdrop.rect.width/backdrop.rect.height;
 var heading=v.masthead.Find("Editable shop heading").GetComponent<TMP_Text>();heading.rectTransform.anchoredPosition=new Vector2(-20,-115);heading.rectTransform.sizeDelta=new Vector2(440,125);heading.fontSize=85;heading.fontSizeMax=85;
 var wallet=v.masthead.Find("Duck coin wallet").GetComponent<Image>();wallet.color=Color.clear;wallet.rectTransform.anchoredPosition=new Vector2(335,-141);wallet.rectTransform.sizeDelta=new Vector2(240,72);v.balance.rectTransform.anchoredPosition=new Vector2(20,0);v.balance.rectTransform.sizeDelta=new Vector2(220,70);
 wallet.transform.Find("Duck footprint coin").gameObject.SetActive(false);
 var ad=v.offers[0];var ads=(RectTransform)ad.purchase.transform.parent;ads.GetComponent<Image>().sprite=art[1];ads.GetComponent<Image>().color=Color.white;ads.sizeDelta=new Vector2(900,310);ads.anchoredPosition=new Vector2(0,-175);
 ad.title.rectTransform.anchoredPosition=new Vector2(70,102);ad.title.rectTransform.sizeDelta=new Vector2(570,75);ad.title.fontSize=53;ad.title.fontSizeMax=53;
 var benefits=ads.Find("Ads benefits").GetComponent<TMP_Text>();benefits.rectTransform.anchoredPosition=new Vector2(150,18);benefits.rectTransform.sizeDelta=new Vector2(520,115);benefits.fontSize=27;benefits.fontSizeMax=27;
 ads.Find("Ads coin emblem").GetComponent<RectTransform>().anchoredPosition=new Vector2(-140,-103);ad.coinAmount.rectTransform.anchoredPosition=new Vector2(-30,-103);ad.coinAmount.rectTransform.sizeDelta=new Vector2(170,65);
 ((RectTransform)ad.purchase.transform).anchoredPosition=new Vector2(275,-103);((RectTransform)ad.purchase.transform).sizeDelta=new Vector2(310,86);
 var plate=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/NamePlate.asset");
 for(int i=0;i<3;i++){var offer=v.offers[i+1];var row=(RectTransform)offer.purchase.transform.parent;row.sizeDelta=new Vector2(900,370);row.anchoredPosition=new Vector2(0,-535-i*390);
 offer.title.rectTransform.anchoredPosition=new Vector2(-135,135);offer.title.rectTransform.sizeDelta=new Vector2(570,70);offer.title.fontSize=49;offer.title.fontSizeMax=49;
 var remove=new System.Collections.Generic.List<GameObject>();foreach(Transform child in row)if(child.name.StartsWith("Reward "))remove.Add(child.gameObject);foreach(var child in remove)UnityEngine.Object.DestroyImmediate(child);
 var coin=ShopImage("Premium coin reward",row,plate,new Vector2(165,238),new Vector2(-340,-30));ShopImage("Premium coin pile",coin.transform,art[10],new Vector2(147,150),new Vector2(0,28));
 offer.coinAmount=ShopLabel("Editable bundle coins",coin.transform,offer.coins.ToString("N0"),37,new Vector2(155,60),new Vector2(0,-78),WoolArt.Hex("78320C"));
 offer.counts=new TMP_Text[4];int count=i+2;for(int k=0;k<count;k++){float x=-155+(k%2)*165;float y=count==2?-35:25-(k/2)*125;
 var cell=ShopImage("Ability reward "+k,row,plate,new Vector2(150,118),new Vector2(x,y));var icon=ShopImage("Ability icon",cell.transform,UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Unlock/LayeredIcon"+k+".asset"),new Vector2(88,82),new Vector2(0,14));icon.preserveAspect=true;
 offer.counts[k]=ShopLabel("Editable ability quantity",cell.transform,"x1",31,new Vector2(115,34),new Vector2(0,-40),WoolArt.Hex("78320C"));}
 ((RectTransform)offer.purchase.transform).anchoredPosition=new Vector2(285,-127);((RectTransform)offer.purchase.transform).sizeDelta=new Vector2(285,88);
 }
 for(int i=0;i<6;i++){var offer=v.offers[i+4];var row=(RectTransform)offer.purchase.transform.parent;row.GetComponent<Image>().sprite=art[5+i%3];row.sizeDelta=new Vector2(286,300);row.anchoredPosition=new Vector2((i%3-1)*306,-1680-(i/3)*320);row.Find("Duck coin").gameObject.SetActive(false);
 offer.coinAmount.rectTransform.anchoredPosition=new Vector2(0,-40);offer.coinAmount.rectTransform.sizeDelta=new Vector2(250,55);offer.coinAmount.fontSize=43;offer.coinAmount.fontSizeMax=43;
 ((RectTransform)offer.purchase.transform).anchoredPosition=new Vector2(0,-104);((RectTransform)offer.purchase.transform).sizeDelta=new Vector2(254,78);}
 var gift=(RectTransform)v.dailyClaim.transform.parent;gift.GetComponent<Image>().sprite=art[8];gift.sizeDelta=new Vector2(900,310);gift.anchoredPosition=new Vector2(0,-2350);gift.Find("Daily gift coin").gameObject.SetActive(false);
 var gt=gift.Find("Editable daily heading").GetComponent<TMP_Text>();gt.rectTransform.anchoredPosition=new Vector2(30,85);gt.rectTransform.sizeDelta=new Vector2(440,75);gt.fontSize=54;gt.fontSizeMax=54;
 var ga=gift.Find("Daily reward amount").GetComponent<TMP_Text>();ga.rectTransform.anchoredPosition=new Vector2(30,13);ga.rectTransform.sizeDelta=new Vector2(430,60);
 var gr=gift.Find("Daily reward rule").GetComponent<TMP_Text>();gr.rectTransform.anchoredPosition=new Vector2(20,-40);gr.rectTransform.sizeDelta=new Vector2(420,45);
 ((RectTransform)v.dailyClaim.transform).anchoredPosition=new Vector2(245,-103);((RectTransform)v.dailyClaim.transform).sizeDelta=new Vector2(330,84);
 foreach(var offer in v.offers){offer.Refresh();PremiumShopButton(offer.purchase);}PremiumShopButton(v.dailyClaim);
 PolishPremiumShopDetails();v.Fit(true);ConfigureInputTargets();UnityEditor.EditorUtility.SetDirty(v);UnityEditor.EditorUtility.SetDirty(this);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
 }

 public void PolishPremiumShopDetails(){
 var v=shopPage.GetComponent<WoolShopView>();var ads=(RectTransform)v.offers[0].purchase.transform.parent;
 var chip=ads.Find("Ad coin bonus chip") as RectTransform;if(!chip)chip=ShopImage("Ad coin bonus chip",ads,UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/NamePlate.asset"),new Vector2(265,74),new Vector2(-35,-104)).rectTransform;
 var icon=ads.Find("Ads coin emblem");if(!icon)icon=chip.Find("Ads coin emblem");icon.SetParent(chip,false);var ir=(RectTransform)icon;ir.anchoredPosition=new Vector2(-95,0);ir.sizeDelta=new Vector2(72,72);
 var amount=v.offers[0].coinAmount;amount.transform.SetParent(chip,false);amount.rectTransform.anchoredPosition=new Vector2(35,0);amount.rectTransform.sizeDelta=new Vector2(170,64);
 for(int i=0;i<6;i++){var b=v.offers[i+4].purchase;var r=(RectTransform)b.transform;r.anchoredPosition=new Vector2(0,-90);r.sizeDelta=new Vector2(240,76);b.GetComponent<Image>().color=Color.clear;var label=b.GetComponentInChildren<TMP_Text>();label.rectTransform.sizeDelta=new Vector2(224,64);
 var row=b.transform.parent;var shine=row.GetComponent<Coffee.UIEffects.UIShiny>();if(!shine)shine=row.gameObject.AddComponent<Coffee.UIEffects.UIShiny>();shine.width=.1f;shine.brightness=.4f;shine.softness=.7f;shine.effectPlayer.play=true;shine.effectPlayer.loop=true;shine.effectPlayer.duration=.9f;shine.effectPlayer.loopDelay=5;shine.effectPlayer.initialPlayDelay=i*.4f;shine.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;
 }
 var gift=v.dailyClaim.transform.parent;gift.Find("Daily reward rule").GetComponent<RectTransform>().anchoredPosition=new Vector2(75,-40);
 UnityEditor.EditorUtility.SetDirty(v);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
 }

 void PremiumShopButton(Button button){
 var im=button.GetComponent<Image>();im.sprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ShopPremium/Purchase.asset");im.type=Image.Type.Simple;im.color=Color.white;
 var text=button.GetComponentInChildren<TMP_Text>(true);text.rectTransform.sizeDelta=((RectTransform)button.transform).sizeDelta-new Vector2(24,12);text.fontSizeMax=40;text.fontSizeMin=25;text.enableAutoSizing=true;
 var shine=button.GetComponent<Coffee.UIEffects.UIShiny>();if(!shine)shine=button.gameObject.AddComponent<Coffee.UIEffects.UIShiny>();shine.width=.12f;shine.softness=.6f;shine.brightness=.6f;shine.gloss=.7f;shine.rotation=25;shine.effectPlayer.play=true;shine.effectPlayer.loop=true;shine.effectPlayer.duration=.8f;shine.effectPlayer.loopDelay=3.5f;shine.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;
 }

 public void AuthorReferenceShop(Sprite[] art){
 var view=shopPage.GetComponent<WoolShopView>();if(!view)view=shopPage.gameObject.AddComponent<WoolShopView>();view.owner=this;view.navigation=menuNavigation;
 foreach(string name in new[]{"Shop awning","Shop wallet"}){var old=shopPage.Find(name);if(old)old.gameObject.SetActive(false);}
 shopPage.GetComponent<Image>().color=WoolArt.Hex("26364E");
 var viewport=shopPage.Find("Shop viewport") as RectTransform;var scrollHit=viewport.GetComponent<Image>();if(!scrollHit)scrollHit=viewport.gameObject.AddComponent<Image>();scrollHit.color=Color.clear;scrollHit.raycastTarget=true;view.viewport=viewport;view.scroll=viewport.GetComponent<ScrollRect>();
 foreach(Transform old in viewport)old.gameObject.SetActive(false);
 var oldHeader=shopPage.Find("Shop masthead");if(oldHeader)UnityEngine.Object.DestroyImmediate(oldHeader.gameObject);
 var head=Rect("Shop masthead",shopPage,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(900,225),Vector2.zero);head.pivot=new Vector2(.5f,1);view.masthead=head;
 ShopImage("Golden awning",head,art[0],new Vector2(900,225),new Vector2(0,-112));
 var title=ShopLabel("Editable shop heading",head,"Shop",78,new Vector2(410,120),new Vector2(-205,-105),Color.white);
 var wallet=ShopImage("Duck coin wallet",head,UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/NamePlate.asset"),new Vector2(345,85),new Vector2(235,-110));
 ShopImage("Duck footprint coin",wallet.transform,skin[5],new Vector2(90,90),new Vector2(-126,0));shopCoins=ShopLabel("Current coins",wallet.transform,"0",45,new Vector2(235,72),new Vector2(35,0),WoolArt.Hex("6D3015"));view.balance=shopCoins;
 var existingCatalog=viewport.Find("Shop catalog content");if(existingCatalog)UnityEngine.Object.DestroyImmediate(existingCatalog.gameObject);var outer=Rect("Shop catalog content",viewport,new Vector2(0,1),new Vector2(1,1),new Vector2(0,WoolShopView.DesignHeight),Vector2.zero);outer.pivot=new Vector2(.5f,1);view.content=outer;
 var design=Rect("Editable shop catalog",outer,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(900,WoolShopView.DesignHeight),Vector2.zero);design.pivot=new Vector2(.5f,1);view.design=design;
 view.scroll.content=outer;view.scroll.viewport=viewport;view.scroll.horizontal=false;view.scroll.vertical=true;view.scroll.movementType=ScrollRect.MovementType.Clamped;view.scroll.inertia=true;view.scroll.decelerationRate=.12f;view.scroll.scrollSensitivity=38;
 var cardSprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/Card.asset");
 var plate=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/NamePlate.asset");
 var offers=new System.Collections.Generic.List<WoolShopOffer>();
 var ads=ShopImage("Remove Ads offer",design,plate,new Vector2(900,250),new Vector2(0,-135));ads.color=WoolArt.Hex("FFE1DF");
 var adTitle=ShopLabel("Ads offer title",ads.transform,"Remove Ads",47,new Vector2(310,70),new Vector2(-265,60),WoolArt.Hex("B42D39"));
 ShopLabel("Ads benefits",ads.transform,"No automatic pop-up ads\nNo bottom banner ads\nReward videos stay optional",28,new Vector2(510,115),new Vector2(165,40),WoolArt.Hex("6D3015"));
 ShopImage("Ads coin emblem",ads.transform,skin[5],new Vector2(100,100),new Vector2(-330,-25));
 var adCoins=ShopLabel("Editable ad coins",ads.transform,"5,000",39,new Vector2(210,70),new Vector2(-210,-40),WoolArt.Hex("793409"));
 var adBuy=ShopBuy(ads.transform,"Rs2,250.00",new Vector2(350,78),new Vector2(235,-77));offers.Add(new WoolShopOffer{bundleName="Remove Ads",price="Rs2,250.00",coins=5000,title=adTitle,coinAmount=adCoins,priceText=adBuy.GetComponentInChildren<TMP_Text>(),purchase=adBuy});
 string[] names={"Starter Bundle","Superior Bundle","Large Bundle"};int[] coins={2000,4000,8000};string[] prices={"Rs550.00","Rs2,250.00","Rs5,700.00"};
 for(int i=0;i<3;i++){
 var row=ShopImage(names[i],design,art[i+1],new Vector2(900,280),new Vector2(0,-420-i*300));
 var name=ShopLabel("Editable bundle name",row.transform,names[i],45,new Vector2(590,70),new Vector2(-120,93),Color.white);
 int slots=3+i;float area=600,step=area/slots,left=-435;var counts=new TMP_Text[4];TMP_Text amount=null;
 for(int k=0;k<slots;k++){float x=left+step*(k+.5f);var cell=ShopImage("Reward "+k,row.transform,plate,new Vector2(step-9,147),new Vector2(x,-14));
 var icon=ShopImage(k==0?"Duck coin reward":"Ability "+(k-1),cell.transform,k==0?skin[5]:UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Unlock/LayeredIcon"+(k-1)+".asset"),new Vector2(Mathf.Min(100,step-18),93),new Vector2(0,17));
 var label=ShopLabel("Editable reward quantity",cell.transform,k==0?coins[i].ToString("N0"):"x1",32,new Vector2(step-15,40),new Vector2(0,-48),WoolArt.Hex("72300C"));if(k==0)amount=label;else counts[k-1]=label;
 }
 var buy=ShopBuy(row.transform,prices[i],new Vector2(270,82),new Vector2(300,-83));var grants=new int[4];for(int k=0;k<i+2;k++)grants[k]=1;
 offers.Add(new WoolShopOffer{bundleName=names[i],price=prices[i],coins=coins[i],abilities=grants,title=name,coinAmount=amount,counts=counts,priceText=buy.GetComponentInChildren<TMP_Text>(),purchase=buy});
 }
 int[] packs={500,1000,2500,5000,10000,25000};string[] packPrices={"Rs150.00","Rs300.00","Rs700.00","Rs1,400.00","Rs2,850.00","Rs6,900.00"};
 for(int i=0;i<6;i++){var row=ShopImage("Coin pack "+packs[i],design,plate,new Vector2(286,250),new Vector2((i%3-1)*306,-1335-(i/3)*270));
 ShopImage("Duck coin",row.transform,skin[5],new Vector2(112,110),new Vector2(0,43));var amount=ShopLabel("Editable pack coins",row.transform,packs[i].ToString("N0"),40,new Vector2(245,50),new Vector2(0,-22),WoolArt.Hex("713014"));var buy=ShopBuy(row.transform,packPrices[i],new Vector2(250,72),new Vector2(0,-83));offers.Add(new WoolShopOffer{bundleName="Coin pack",coins=packs[i],price=packPrices[i],coinAmount=amount,purchase=buy,priceText=buy.GetComponentInChildren<TMP_Text>()});}
 var gift=ShopImage("Daily free 50 coins",design,plate,new Vector2(900,250),new Vector2(0,-1900));
 ShopLabel("Editable daily heading",gift.transform,"Daily Gift",50,new Vector2(520,78),new Vector2(60,69),WoolArt.Hex("733714"));
 ShopImage("Daily gift coin",gift.transform,skin[5],new Vector2(170,170),new Vector2(-315,0));
 ShopLabel("Daily reward amount",gift.transform,"50 FREE COINS",41,new Vector2(530,68),new Vector2(75,5),WoolArt.Hex("783A13"));
 ShopLabel("Daily reward rule",gift.transform,"A little gift, every day",27,new Vector2(510,45),new Vector2(65,-48),WoolArt.Hex("9A6431"));
 var claim=ShopBuy(gift.transform,"CLAIM FREE",new Vector2(360,76),new Vector2(165,-92));view.dailyClaim=claim;view.claimText=claim.GetComponentInChildren<TMP_Text>();claim.name="Claim Daily Shop Gift";
 view.offers=offers.ToArray();view.Fit(true);ConfigureInputTargets();UnityEditor.EditorUtility.SetDirty(view);UnityEditor.EditorUtility.SetDirty(this);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
 }
 Image ShopImage(string name,Transform parent,Sprite sprite,Vector2 size,Vector2 position){var anchor=(parent.name=="Editable shop catalog"||parent.name=="Shop masthead")?new Vector2(.5f,1):new Vector2(.5f,.5f);var r=Rect(name,parent,anchor,anchor,size,position);var im=r.gameObject.AddComponent<Image>();im.sprite=sprite;im.raycastTarget=false;return im;}
 TMP_Text ShopLabel(string name,Transform parent,string text,float size,Vector2 area,Vector2 position,Color color){var label=Label(name,parent,text,(int)size,area,position,color);if(parent.name=="Shop masthead"){label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,1);label.rectTransform.anchoredPosition=position;}label.enableAutoSizing=true;label.fontSizeMin=size*.75f;label.fontSizeMax=size;label.fontSharedMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/Victory/ButtonText.mat");label.color=color;return label;}
 Button ShopBuy(Transform parent,string price,Vector2 size,Vector2 pos){var b=ButtonAt("Purchase",parent,price,size,pos,ShopPurchaseUnavailable);var im=b.GetComponent<Image>();im.sprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/Victory/VictoryButton2.png");im.type=Image.Type.Simple;im.color=Color.white;var label=b.GetComponentInChildren<TMP_Text>();label.rectTransform.sizeDelta=size-new Vector2(20,10);label.fontSharedMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/Victory/ButtonText.mat");label.fontSize=38;label.fontSizeMin=26;label.fontSizeMax=38;label.enableAutoSizing=true;label.color=Color.white;return b;}
#endif
}
}
