using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace WoolGame {
public sealed class WoolAbilityFocus:MonoBehaviour {
 public RectTransform hand,halo;public Vector2 handOrigin;float clock;
 void OnEnable(){clock=0;}
 void Update(){clock+=Time.unscaledDeltaTime;if(hand){hand.anchoredPosition=handOrigin;hand.localScale=Vector3.one*(.72f*(1+.06f*Mathf.Sin(clock*5)));}if(halo){halo.localRotation=Quaternion.Euler(0,0,clock*24);halo.localScale=Vector3.one*(1+.12f*Mathf.Sin(clock*5));}}
}
public sealed partial class WoolHud {
 GameObject abilityGuide,abilityEffect;Button guideTap;TMP_Text guideCopy,guideTitle;Image guideIcon;RectTransform guideHand,guideHalo,guideMessage,levelPointerRoot,levelPointerHand,levelPointerFrame;int abilityStage;
 RectTransform guideTarget;Transform guideWorldTarget;Camera guideWorldCamera;Vector2 guideScreen,guideSize;
 readonly Vector3[] guideCorners=new Vector3[4];
 Button levelPointerTap;WoolAbilitySpotlight levelSpotlight;Action levelTapAction;
 Camera GuideCamera=>canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
 public bool AbilityGuideVisible=>abilityGuide&&abilityGuide.activeSelf;
 public void SetAbilityLevels(int stage){abilityStage=stage;if(!footer)return;for(int k=0;k<4;k++){var b=footer.Find(WoolBoosters.Names[k]);if(!b)continue;foreach(var shine in b.GetComponentsInChildren<Coffee.UIEffects.UIShiny>(true))shine.enabled=false;bool available=WoolBoosters.Available(k,stage);b.GetComponent<Button>().interactable=available;var shade=b.GetComponent<CanvasGroup>();if(!shade)shade=b.gameObject.AddComponent<CanvasGroup>();shade.alpha=available?1:.8f;if(abilityStrip&&k<abilityStrip.counts.Length&&abilityStrip.counts[k])abilityStrip.counts[k].text=available?(WoolProgress.AbilityCount(k)>0?WoolProgress.AbilityCount(k).ToString():"+"):"Lv "+WoolBoosters.UnlockLevels[k];}}
 void BuildAbilityGuide(){
 if(abilityGuide)return;
 var root=Rect("Guided ability spotlight",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);abilityGuide=root.gameObject;
 var layer=root.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=200;root.gameObject.AddComponent<GraphicRaycaster>();
 var block=root.gameObject.AddComponent<WoolAbilitySpotlight>();block.color=new Color(.035f,.04f,.08f,.66f);block.raycastTarget=true;
 var focus=root.gameObject.AddComponent<WoolAbilityFocus>();
 guideHalo=Rect("Focused real ability button",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(250,250),Vector2.zero);var frame=guideHalo.gameObject.AddComponent<WoolTutorialFrame>();frame.color=WoolArt.Hex("30DFFF");frame.raycastTarget=false;
 guideTap=ButtonAt("Use highlighted free ability",root,"",new Vector2(180,135),Vector2.zero,null);guideTap.GetComponent<Image>().sprite=null;guideTap.GetComponent<Image>().color=Color.clear;
 var feedback=guideTap.GetComponent<WoolButtonAudio>();if(feedback)feedback.enabled=false;
 var colors=guideTap.colors;colors.normalColor=colors.highlightedColor=colors.pressedColor=colors.selectedColor=Color.white;guideTap.colors=colors;
 guideMessage=Panel("Ability explanation card",root,new Vector2(.5f,.5f),new Vector2(800,250),Vector2.zero,Color.white);
 if(skin!=null&&skin.Length>0)guideMessage.GetComponent<Image>().sprite=skin[0];
 guideMessage.GetComponent<Image>().raycastTarget=false;
 guideTitle=Label("Power name and free try",guideMessage,"FREE TRY",32,new Vector2(600,44),new Vector2(50,58),WoolArt.Hex("157378"));guideTitle.fontStyle=FontStyles.Bold;
 guideCopy=Label("One action",guideMessage,"",50,new Vector2(640,112),new Vector2(50,-20),WoolArt.Hex("59361E"));guideCopy.enableAutoSizing=false;guideCopy.fontStyle=FontStyles.Bold;
 guideIcon=Panel("Recognise this power",guideMessage,new Vector2(.5f,.5f),new Vector2(110,110),new Vector2(-320,0),Color.white).GetComponent<Image>();guideIcon.preserveAspect=true;guideIcon.raycastTarget=false;
 guideHand=Rect("Animated tutorial hand",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(165,185),Vector2.zero);var hand=guideHand.gameObject.AddComponent<WoolWarehouseHand>();hand.color=WoolArt.Hex("30DFFF");hand.raycastTarget=false;focus.hand=guideHand;
 abilityGuide.SetActive(false);
 }
 Vector2 GuidePoint(Vector2 screen){var parent=abilityGuide?(RectTransform)abilityGuide.transform:(RectTransform)transform;RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,screen,GuideCamera,out var p);return p;}
 public void ShowAbilityGuide(int kind,string message,Action use){
 BuildAbilityGuide();guideTarget=footer.Find(WoolBoosters.Names[kind]) as RectTransform;guideWorldTarget=null;
 guideTitle.text=WoolBoosters.Names[kind].ToUpperInvariant()+"  ·  FREE TRY";
 guideIcon.sprite=unlockPresentation.abilityIcons[kind];guideIcon.gameObject.SetActive(true);
 PlaceGuide(Vector2.zero,message,use,new Vector2(150,150));
 }
 public void ShowWorldAbilityGuide(Vector2 screen,string message,Action use){BuildAbilityGuide();guideTarget=null;guideWorldTarget=null;guideTitle.text="CHOOSE A VEHICLE";guideIcon.gameObject.SetActive(false);PlaceGuide(screen,message,use,new Vector2(150,150));}
 public void ShowWorldAbilityGuide(Transform target,Camera camera,string message,Action use){BuildAbilityGuide();guideTarget=null;guideWorldTarget=target;guideWorldCamera=camera;guideTitle.text="CLEAR THE WAY";guideIcon.gameObject.SetActive(false);PlaceGuide(camera.WorldToScreenPoint(target.position),message,use,new Vector2(150,150));}
 void PlaceGuide(Vector2 screen,string message,Action use,Vector2 buttonSize){
 abilityGuide.SetActive(true);abilityGuide.transform.SetAsLastSibling();Canvas.ForceUpdateCanvases();
 guideScreen=screen;guideSize=buttonSize;guideCopy.text=message;
 guideTap.onClick.RemoveAllListeners();bool used=false;guideTap.onClick.AddListener(()=>{if(used)return;used=true;use?.Invoke();});
 RefreshTutorialOverlays();
 }
 void RefreshTutorialOverlays(){
 if(!AbilityGuideVisible)return;
 var root=(RectTransform)abilityGuide.transform;
 // The overlay is full-screen. Convert the final, safe-area-adjusted target AFTER footer layout.
 root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
 if(guideTarget){guideTarget.GetWorldCorners(guideCorners);var a=GuidePoint(RectTransformUtility.WorldToScreenPoint(GuideCamera,guideCorners[0]));var b=GuidePoint(RectTransformUtility.WorldToScreenPoint(GuideCamera,guideCorners[2]));guideScreen=RectTransformUtility.WorldToScreenPoint(GuideCamera,(guideCorners[0]+guideCorners[2])*.5f);guideSize=b-a;}
 if(guideWorldTarget&&guideWorldCamera)guideScreen=guideWorldCamera.WorldToScreenPoint(guideWorldTarget.position);
 var point=GuidePoint(guideScreen);var buttonSize=guideSize;
 var cutout=abilityGuide.GetComponent<WoolAbilitySpotlight>();cutout.opening=new Rect(point-buttonSize*.5f-Vector2.one*12,buttonSize+Vector2.one*24);cutout.SetVerticesDirty();
 guideHalo.anchoredPosition=point;guideHalo.sizeDelta=buttonSize+Vector2.one*24;
 var tap=(RectTransform)guideTap.transform;tap.localScale=Vector3.one;tap.anchoredPosition=point;tap.sizeDelta=buttonSize;guideTap.GetComponentInChildren<TMP_Text>().text="";
 var safeMin=GuidePoint(CurrentSafeArea.min);var safeMax=GuidePoint(CurrentSafeArea.max);
 bool above=point.y<(safeMin.y+safeMax.y)*.5f;
 var tip=point+new Vector2(0,(above?1:-1)*(buttonSize.y*.5f+18));
 guideHand.anchoredPosition=tip;guideHand.localRotation=Quaternion.Euler(0,0,above?180:0);abilityGuide.GetComponent<WoolAbilityFocus>().handOrigin=tip;
 float w=Mathf.Min(920,safeMax.x-safeMin.x-48),height=210;
 float y=point.y+(above?1:-1)*(buttonSize.y*.5f+height*.5f+150);
 guideMessage.sizeDelta=new Vector2(w,height);guideMessage.anchoredPosition=new Vector2(Mathf.Clamp(point.x,safeMin.x+w*.5f+12,safeMax.x-w*.5f-12),Mathf.Clamp(y,safeMin.y+height*.5f+16,safeMax.y-height*.5f-16));
 bool icon=guideIcon.gameObject.activeSelf;float inset=icon?146:28;
 guideIcon.rectTransform.anchoredPosition=new Vector2(-w*.5f+84,0);
 guideTitle.rectTransform.sizeDelta=new Vector2(w-inset-28,44);guideTitle.rectTransform.anchoredPosition=new Vector2(inset*.5f-14,64);
 guideCopy.rectTransform.sizeDelta=new Vector2(w-inset-28,122);guideCopy.rectTransform.anchoredPosition=new Vector2(inset*.5f-14,-20);
 }
 void BuildLevelPointer(){
 if(levelPointerRoot)return;
 levelPointerRoot=Rect("Level tutorial pointer",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
 var layer=levelPointerRoot.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=250;levelPointerRoot.gameObject.AddComponent<GraphicRaycaster>();
 levelSpotlight=levelPointerRoot.gameObject.AddComponent<WoolAbilitySpotlight>();levelSpotlight.color=new Color(.025f,.04f,.065f,.55f);levelSpotlight.raycastTarget=true;
 levelPointerFrame=Rect("Tap this vehicle",levelPointerRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(150,150),Vector2.zero);var frame=levelPointerFrame.gameObject.AddComponent<WoolTutorialFrame>();frame.color=WoolArt.Hex("30DFFF");frame.raycastTarget=false;
 levelPointerTap=ButtonAt("Only the tutorial target accepts taps",levelPointerRoot,"",new Vector2(150,150),Vector2.zero,null);levelPointerTap.GetComponent<Image>().sprite=null;levelPointerTap.GetComponent<Image>().color=Color.clear;
 var feedback=levelPointerTap.GetComponent<WoolButtonAudio>();if(feedback)feedback.enabled=false;
 levelPointerTap.onClick.AddListener(()=>levelTapAction?.Invoke());
 levelPointerHand=Rect("Cyan pointing hand",levelPointerRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(165,185),Vector2.zero);var hand=levelPointerHand.gameObject.AddComponent<WoolWarehouseHand>();hand.color=WoolArt.Hex("30DFFF");hand.raycastTarget=false;
 }
 void ActivateLevelPointer(){BuildLevelPointer();levelPointerRoot.gameObject.SetActive(true);levelPointerRoot.SetAsLastSibling();levelPointerRoot.anchorMin=Vector2.zero;levelPointerRoot.anchorMax=Vector2.one;levelPointerRoot.offsetMin=levelPointerRoot.offsetMax=Vector2.zero;}
 public void BlockLevelTutorial(){ActivateLevelPointer();levelSpotlight.opening=new Rect(Vector2.zero,Vector2.zero);levelSpotlight.SetVerticesDirty();levelPointerFrame.gameObject.SetActive(false);levelPointerHand.gameObject.SetActive(false);levelPointerTap.gameObject.SetActive(false);levelTapAction=null;}
 public void PointLevelTutorialAt(Vector2 screen,bool visible,Vector2 screenSize=default,Action tap=null){
 if(!visible){if(levelPointerRoot)levelPointerRoot.gameObject.SetActive(false);return;}
 ActivateLevelPointer();levelTapAction=tap;
 if(screenSize==Vector2.zero)screenSize=Vector2.one*(150*canvas.scaleFactor);
 RectTransformUtility.ScreenPointToLocalPointInRectangle(levelPointerRoot,screen,GuideCamera,out var point);
 RectTransformUtility.ScreenPointToLocalPointInRectangle(levelPointerRoot,screen-screenSize*.5f,GuideCamera,out var a);RectTransformUtility.ScreenPointToLocalPointInRectangle(levelPointerRoot,screen+screenSize*.5f,GuideCamera,out var b);
 var size=b-a;levelSpotlight.opening=new Rect(point-size*.5f-Vector2.one*10,size+Vector2.one*20);levelSpotlight.SetVerticesDirty();
 levelPointerFrame.gameObject.SetActive(true);levelPointerFrame.sizeDelta=size+Vector2.one*20;levelPointerFrame.anchoredPosition=point;
 levelPointerTap.gameObject.SetActive(true);var button=(RectTransform)levelPointerTap.transform;button.localScale=Vector3.one;button.sizeDelta=size;button.anchoredPosition=point;
 levelPointerHand.gameObject.SetActive(true);levelPointerHand.anchoredPosition=point+Vector2.down*(size.y*.5f+18);levelPointerHand.localScale=Vector3.one*(.72f*(1+.05f*Mathf.Sin(Time.unscaledTime*4)));levelPointerHand.localRotation=Quaternion.identity;
 }
 RectTransform removalInstruction;
 void ShowRemovalInstruction(bool visible){
 if(!removalInstruction&&visible){removalInstruction=Panel("Remove power - choose a vehicle",safe,new Vector2(.5f,.65f),new Vector2(850,76),Vector2.zero,Color.white);if(skin!=null&&skin.Length>0)removalInstruction.GetComponent<Image>().sprite=skin[0];removalInstruction.GetComponent<Image>().raycastTarget=false;var label=Label("Remove instruction",removalInstruction,"Tap a vehicle. Clear its matching dragon colour.",32,new Vector2(800,64),Vector2.zero,WoolArt.Hex("59361E"));label.enableAutoSizing=true;label.fontSizeMin=26;label.fontSizeMax=32;}
 if(removalInstruction)removalInstruction.gameObject.SetActive(visible);
 }
 public void HideAbilityGuide(){if(abilityGuide)abilityGuide.SetActive(false);}
 public void ShowAbilityEffect(int kind,Vector2 screen){
 HideAbilityEffect();var root=Rect("Ability activation feedback",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);abilityEffect=root.gameObject;root.SetAsLastSibling();
 Canvas.ForceUpdateCanvases();RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,null,out var point);
 var ring=Rect("Effect at its real destination",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(290,290),point);ring.gameObject.AddComponent<WoolAbilityGlow>().raycastTarget=false;root.gameObject.AddComponent<WoolAbilityFocus>().halo=ring;
 float width=Mathf.Min(820,root.rect.width-72);var card=Panel("Power purpose",root,new Vector2(.5f,.5f),new Vector2(width,152),new Vector2(0,root.rect.height*.23f),Color.white);card.GetComponent<Image>().raycastTarget=false;if(skin!=null&&skin.Length>0)card.GetComponent<Image>().sprite=skin[0];
 var icon=Panel("Power illustration",card,new Vector2(.5f,.5f),new Vector2(110,110),new Vector2(-width*.5f+80,0),Color.white);icon.GetComponent<Image>().sprite=unlockPresentation.abilityIcons[kind];icon.GetComponent<Image>().preserveAspect=true;icon.GetComponent<Image>().raycastTarget=false;
 string[] titles={"SLOT READY!","VEHICLE REMOVED!","COLOURS GROUPED!","SHOOT THROUGH FOG: 12s"};
 var title=Label("Power result",card,titles[kind],44,new Vector2(width-190,110),new Vector2(65,0),WoolArt.Hex("59361E"));title.enableAutoSizing=false;title.fontStyle=FontStyles.Bold;
 }
 public void HideAbilityEffect(){if(abilityEffect){Destroy(abilityEffect);abilityEffect=null;}}
 }
}
