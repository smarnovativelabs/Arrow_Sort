using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed class WoolWarehouseTutorial:MonoBehaviour {
 public RectTransform card,frontVehicle,nextVehicle,hand;
 public CanvasGroup frontFade,nextFade,handFade;public TMP_Text remaining;
 public Button continueButton,closeButton;float clock,closeAt;Action dismiss;
 public int CompletedLoops=>Mathf.FloorToInt(clock/5f);
 public float CycleTime=>Mathf.Repeat(clock,5f);
 public void Open(Action closed){clock=0;closeAt=Time.unscaledTime+10;dismiss=closed;continueButton.onClick.RemoveAllListeners();closeButton.onClick.RemoveAllListeners();continueButton.onClick.AddListener(Close);closeButton.onClick.AddListener(Close);Sample(0);Fit();}
 void Close(){var callback=dismiss;dismiss=null;callback?.Invoke();}
 void OnDisable(){dismiss=null;clock=closeAt=0;}
 void Update(){if(closeAt>0&&Time.unscaledTime>=closeAt){Close();return;}clock=Mathf.Max(0,Time.unscaledTime-(closeAt-10));Sample(CycleTime);Fit();}
 void Fit(){var root=(RectTransform)transform;float scale=Mathf.Max(.1f,Mathf.Min((root.rect.width-56)/760,(root.rect.height-80)/1240));card.localScale=Vector3.one*scale;card.anchoredPosition=new Vector2(0,-35*scale);}
 public void Sample(float t){
 t=Mathf.Repeat(t,5);float depart=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1,2.15f,t));
 frontVehicle.anchoredPosition=new Vector2(0,80+depart*330);frontFade.alpha=t<2.15f?1-Mathf.InverseLerp(1.7f,2.15f,t):0;
 float reveal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.3f,3.35f,t));nextVehicle.anchoredPosition=new Vector2(0,Mathf.Lerp(-155,80,reveal));nextFade.alpha=t<2.3f?0:1;
 remaining.text=t<2.3f?"6":"5";
 float approach=Mathf.SmoothStep(0,1,t/.7f);hand.anchoredPosition=Vector2.Lerp(new Vector2(180,-55),new Vector2(0,85),approach);
 handFade.alpha=t<1.4f?1:1-Mathf.InverseLerp(1.4f,1.8f,t);float press=t>.7f&&t<1?1-.09f*Mathf.Sin((t-.7f)/.3f*Mathf.PI):1;hand.localScale=Vector3.one*press;
 }
}
public static partial class WoolProgress{
 public static bool WarehouseTutorialSeen{get{EnsureLoaded();return data.warehouseTutorialSeen;}}
 public static void MarkWarehouseTutorialSeen(){EnsureLoaded();if(data.warehouseTutorialSeen)return;data.warehouseTutorialSeen=true;Save();}
}
public sealed partial class WoolHud{
 [SerializeField] WoolWarehouseTutorial warehouseTutorial;
 public void ShowWarehouseTutorial(){if(!warehouseTutorial||WoolProgress.WarehouseTutorialSeen)return;RestoreWinPresentation();RestoreUnlockPresentation();modalCard.gameObject.SetActive(false);modal.GetComponent<Image>().color=new Color(0,0,0,.84f);modal.SetActive(true);modal.transform.SetAsLastSibling();warehouseTutorial.gameObject.SetActive(true);warehouseTutorial.transform.SetAsLastSibling();warehouseTutorial.Open(()=>{WoolProgress.MarkWarehouseTutorialSeen();Hide();});}
 void RestoreWarehouseTutorial(){if(warehouseTutorial)warehouseTutorial.gameObject.SetActive(false);if(modalCard)modalCard.gameObject.SetActive(true);}
#if UNITY_EDITOR

 public void AuthorWarehouseReferenceV2(Sprite[] art){
 var p=warehouseTutorial;if(!p)throw new InvalidOperationException("Warehouse panel missing");
 p.card.GetComponent<Image>().sprite=art[0];p.card.sizeDelta=new Vector2(760,1080);
 var tag=p.card.Find("Warehouse heading").GetComponent<Image>();tag.sprite=art[1];tag.rectTransform.sizeDelta=new Vector2(600,320);tag.rectTransform.anchoredPosition=new Vector2(0,480);
 var title=tag.GetComponentInChildren<TMP_Text>(true);title.rectTransform.anchoredPosition=new Vector2(0,-75);title.rectTransform.sizeDelta=new Vector2(530,95);title.text="Warehouse";
 var font=UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/WoolGame/Fonts/LilitaOne-HomeSharp.asset");
 foreach(var text in p.GetComponentsInChildren<TMP_Text>(true)){text.font=font;text.fontSharedMaterial=font.material;}
 string matPath="Assets/WoolGame/Designer/UI/WarehouseReferenceV2/Title.mat";var mat=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(matPath);if(!mat){mat=new Material(font.material);mat.SetFloat("_OutlineWidth",.13f);mat.SetColor("_OutlineColor",WoolArt.Hex("71310F"));UnityEditor.AssetDatabase.CreateAsset(mat,matPath);}
 title.fontSharedMaterial=mat;title.color=Color.white;title.fontSize=60;title.fontSizeMax=60;title.fontSizeMin=48;title.enableAutoSizing=true;
 var body=p.card.Find("Editable warehouse explanation").GetComponent<TMP_Text>();body.text="Move the front vehicle away to reveal\nthe next vehicle in the warehouse.";body.rectTransform.anchoredPosition=new Vector2(0,307);body.rectTransform.sizeDelta=new Vector2(650,125);body.fontSize=34;body.fontSizeMax=34;body.fontSizeMin=29;body.color=WoolArt.Hex("793D20");
 var stage=p.card.Find("Looping warehouse demo").GetComponent<Image>();stage.sprite=art[2];stage.rectTransform.anchoredPosition=new Vector2(0,-45);stage.rectTransform.sizeDelta=new Vector2(630,560);
 var depot=stage.transform.Find("Warehouse container").GetComponent<Image>();depot.sprite=art[6];depot.rectTransform.sizeDelta=new Vector2(160,230);depot.rectTransform.anchoredPosition=new Vector2(0,-150);depot.preserveAspect=true;
 p.remaining.rectTransform.anchoredPosition=new Vector2(0,-110);p.remaining.rectTransform.sizeDelta=new Vector2(100,80);p.remaining.fontSharedMaterial=mat;p.remaining.fontSize=56;p.remaining.fontSizeMax=56;p.remaining.fontSizeMin=56;p.remaining.color=Color.white;
 p.frontVehicle.sizeDelta=p.nextVehicle.sizeDelta=new Vector2(112,190);
 p.frontVehicle.GetComponent<Image>().sprite=art[4];p.nextVehicle.GetComponent<Image>().sprite=art[5];
 foreach(var car in new[]{p.frontVehicle,p.nextVehicle}){car.GetComponent<Image>().preserveAspect=true;var arrow=car.Find("Vehicle direction arrow");if(arrow)arrow.gameObject.SetActive(false);}
 var handGraphic=p.hand.GetComponent<WoolWarehouseHand>();if(handGraphic)handGraphic.enabled=false;var handArt=p.hand.Find("Hand illustration") as RectTransform;if(!handArt)handArt=Rect("Hand illustration",p.hand,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var handImage=handArt.GetComponent<Image>();if(!handImage)handImage=handArt.gameObject.AddComponent<Image>();handImage.sprite=art[7];handImage.raycastTarget=false;handImage.preserveAspect=true;p.hand.sizeDelta=new Vector2(165,195);p.hand.pivot=new Vector2(.06f,.96f);
 p.continueButton.GetComponent<Image>().sprite=art[3];((RectTransform)p.continueButton.transform).sizeDelta=new Vector2(540,125);((RectTransform)p.continueButton.transform).anchoredPosition=new Vector2(0,-440);
 var label=p.continueButton.GetComponentInChildren<TMP_Text>();label.fontSharedMaterial=mat;label.fontSize=51;label.fontSizeMax=51;label.fontSizeMin=45;label.color=Color.white;label.rectTransform.sizeDelta=new Vector2(490,100);
 ((RectTransform)p.closeButton.transform).anchoredPosition=new Vector2(326,480);((RectTransform)p.closeButton.transform).sizeDelta=new Vector2(65,65);
 var dim=p.transform.Find("Warehouse dark backdrop") as RectTransform;if(!dim){dim=Rect("Warehouse dark backdrop",p.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);dim.gameObject.AddComponent<Image>();}dim.anchorMin=Vector2.zero;dim.anchorMax=Vector2.one;dim.offsetMin=dim.offsetMax=Vector2.zero;dim.GetComponent<Image>().color=new Color(0,0,0,.55f);dim.GetComponent<Image>().raycastTarget=true;dim.SetAsFirstSibling();
 ConfigureInputTargets();p.gameObject.SetActive(false);UnityEditor.EditorUtility.SetDirty(p);UnityEditor.EditorUtility.SetDirty(this);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
 }

 public void AuthorWarehouseTutorial(Sprite first,Sprite second,Sprite container){
 if(warehouseTutorial)return;
 var root=Rect("First warehouse tutorial",modal.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);warehouseTutorial=root.gameObject.AddComponent<WoolWarehouseTutorial>();var p=warehouseTutorial;
 var frame=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/Card.asset");
 var plate=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/NamePlate.asset");
 var banner=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ProfileEditor/Banner.asset");
 var mat=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/Victory/ButtonText.mat");
 var card=Panel("Warehouse information",root,new Vector2(.5f,.5f),new Vector2(760,1080),Vector2.zero,Color.white);card.GetComponent<Image>().sprite=frame;p.card=card;
 var titleArt=Panel("Warehouse heading",card,new Vector2(.5f,.5f),new Vector2(690,135),new Vector2(0,465),Color.white);titleArt.GetComponent<Image>().sprite=banner;
 var title=Label("Editable warehouse title",titleArt,"Warehouse",61,new Vector2(615,100),Vector2.zero,Color.white);title.fontSharedMaterial=mat;
 var body=Label("Editable warehouse explanation",card,"Move the front vehicle away.\nThe next vehicle will come out.",34,new Vector2(650,130),new Vector2(0,325),WoolArt.Hex("783816"));body.enableAutoSizing=true;body.fontSizeMin=26;body.fontSizeMax=34;
 var stage=Panel("Looping warehouse demo",card,new Vector2(.5f,.5f),new Vector2(630,560),new Vector2(0,-40),Color.white);stage.GetComponent<Image>().sprite=plate;stage.gameObject.AddComponent<RectMask2D>();
 var next=WarehouseVehicle("Next vehicle",stage,second);p.nextVehicle=next;p.nextFade=next.gameObject.AddComponent<CanvasGroup>();
 var depot=Rect("Warehouse container",stage,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(380,230),new Vector2(0,-160));var di=depot.gameObject.AddComponent<Image>();di.sprite=container;di.preserveAspect=true;di.raycastTarget=false;
 p.remaining=Label("Demo remaining vehicles",stage,"6\n<size=45%>LEFT</size>",48,new Vector2(220,120),new Vector2(0,-180),Color.white);p.remaining.fontSharedMaterial=mat;
 var firstCar=WarehouseVehicle("Front vehicle",stage,first);p.frontVehicle=firstCar;p.frontFade=firstCar.gameObject.AddComponent<CanvasGroup>();
 var hand=Rect("Animated tapping hand",stage,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(220,220),new Vector2(0,85));hand.pivot=new Vector2(0,1);hand.gameObject.AddComponent<CanvasRenderer>();hand.gameObject.AddComponent<WoolWarehouseHand>().raycastTarget=false;p.hand=hand;p.handFade=hand.gameObject.AddComponent<CanvasGroup>();
 p.continueButton=ButtonAt("Warehouse Continue",card,"Continue",new Vector2(580,112),new Vector2(0,-440),null);p.continueButton.GetComponent<Image>().sprite=UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WoolGame/Designer/UI/ShopPremium/Purchase.asset");
 var ct=p.continueButton.GetComponentInChildren<TMP_Text>();ct.fontSize=47;ct.fontSizeMax=47;ct.fontSharedMaterial=mat;ct.color=Color.white;
 p.closeButton=ButtonAt("Close warehouse information",card,"X",new Vector2(80,80),new Vector2(328,490),null);p.closeButton.GetComponent<Image>().sprite=plate;p.closeButton.GetComponent<Image>().color=WoolArt.Hex("F04735");p.closeButton.GetComponentInChildren<TMP_Text>().color=Color.white;
 root.gameObject.SetActive(false);ConfigureInputTargets();UnityEditor.EditorUtility.SetDirty(p);UnityEditor.EditorUtility.SetDirty(this);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
 }
 RectTransform WarehouseVehicle(string name,Transform parent,Sprite sprite){
 var r=Rect(name,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(165,185),new Vector2(0,80));var im=r.gameObject.AddComponent<Image>();im.sprite=sprite;im.raycastTarget=false;
 var arrow=Rect("Vehicle direction arrow",r,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(75,90),new Vector2(0,-5));arrow.gameObject.AddComponent<CanvasRenderer>();arrow.gameObject.AddComponent<WoolWarehouseArrow>().raycastTarget=false;return r;
 }
#endif
}
}
