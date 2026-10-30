using System;using UnityEngine;using UnityEngine.UI;using TMPro;
namespace WoolGame {public sealed partial class WoolHud {
 Action gameplayRuleLayout;GameObject gameplayRule;public bool GameplayRuleVisible=>gameplayRule&&gameplayRule.activeSelf;
 public void RefreshGameplayRule(){if(GameplayRuleVisible)gameplayRuleLayout?.Invoke();}
 public void HideGameplayRule(){gameplayRuleLayout=null;if(gameplayRule)Destroy(gameplayRule);gameplayRule=null;}
 public void ShowGameplayRule(WoolSceneRig rig,bool smoke,Action next){
  HideGameplayRule();var root=Rect("Gameplay rule spotlight",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);gameplayRule=root.gameObject;
  var layer=root.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=280;root.gameObject.AddComponent<GraphicRaycaster>();Canvas.ForceUpdateCanvases();
  var shade=root.gameObject.AddComponent<WoolAbilitySpotlight>();shade.color=new Color(.025f,.035f,.07f,.76f);shade.raycastTarget=true;
  var pixels=rig.SlotCamera.pixelRect;
  if(smoke){pixels=rig.gameCamera.pixelRect;float fogY=rig.authoredLayout.fogBoundary?rig.gameCamera.WorldToScreenPoint(rig.authoredLayout.fogBoundary.position).y:pixels.yMax-pixels.height*.2f;pixels.yMin=Mathf.Clamp(fogY,pixels.yMin,pixels.yMax-pixels.height*.16f);}
  RectTransformUtility.ScreenPointToLocalPointInRectangle(root,pixels.min,GuideCamera,out var min);RectTransformUtility.ScreenPointToLocalPointInRectangle(root,pixels.max,GuideCamera,out var max);shade.opening=UnityEngine.Rect.MinMaxRect(min.x,min.y,max.x,max.y);shade.SetVerticesDirty();
  var focus=Rect("Highlighted rule area",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),max-min,(min+max)*.5f);var frame=focus.gameObject.AddComponent<WoolTutorialFrame>();frame.color=WoolArt.Hex("30DFFF");frame.raycastTarget=false;
  var safeMin=GuideLocal(CurrentSafeArea.min,root);var safeMax=GuideLocal(CurrentSafeArea.max,root);float w=Mathf.Min(860,safeMax.x-safeMin.x-64),height=380;float y=Mathf.Clamp(min.y-height*.5f-42,safeMin.y+height*.5f+140,safeMax.y-height*.5f-24);
  var card=Panel("Rule explanation",root,new Vector2(.5f,.5f),new Vector2(w,height),new Vector2((safeMin.x+safeMax.x)*.5f,y),new Color(1,.97f,.87f));if(skin!=null&&skin.Length>0)card.GetComponent<Image>().sprite=skin[0];
  var title=Label("Rule title",card,smoke?"SMOKE BLOCKS YOUR SHOTS":"START WITH 4 SLOTS",38,new Vector2(w-56,60),new Vector2(0,118),WoolArt.Hex("157378"));title.fontStyle=FontStyles.Bold;title.enableAutoSizing=true;title.fontSizeMin=26;title.fontSizeMax=38;
  var step=Label("Rule step",card,smoke?"2 / 2":"1 / 2",22,new Vector2(w-56,30),new Vector2(0,164),WoolArt.Hex("157378"));step.fontStyle=FontStyles.Bold;
  var copy=Label("Rule text",card,smoke?"Wait for snake colours to leave the smoke.\nPierce Fog lets you shoot through it.":"Place up to 4 vehicles.\nGet more slots with AD + or Unlock.",38,new Vector2(w-72,152),new Vector2(0,8),WoolArt.Hex("59361E"));copy.enableAutoSizing=true;copy.fontSizeMin=30;copy.fontSizeMax=38;
  var button=ButtonAt("Continue gameplay rule",card,smoke?"GOT IT":"NEXT",new Vector2(Mathf.Min(400,w-88),84),new Vector2(0,-132),()=>next?.Invoke());var caption=button.GetComponentInChildren<TMP_Text>();caption.fontSize=38;caption.fontStyle=FontStyles.Bold;caption.color=Color.white;button.transform.SetAsLastSibling();
  Canvas.ForceUpdateCanvases();var br=(RectTransform)button.transform;var tip=root.InverseTransformPoint(br.TransformPoint(new Vector3(br.rect.center.x,br.rect.yMin-18,0)));var hand=Rect("Hand pointing at Next",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(165,185),tip);var graphic=hand.gameObject.AddComponent<WoolWarehouseHand>();graphic.color=WoolArt.Hex("30DFFF");graphic.raycastTarget=false;var pulse=hand.gameObject.AddComponent<WoolAbilityFocus>();pulse.hand=hand;pulse.handOrigin=tip;
  gameplayRuleLayout=()=>{
   var view=smoke?rig.gameCamera.pixelRect:rig.SlotCamera.pixelRect;
   if(smoke){float fogY=rig.authoredLayout.fogBoundary?rig.gameCamera.WorldToScreenPoint(rig.authoredLayout.fogBoundary.position).y:view.yMax-view.height*.2f;view.yMin=Mathf.Clamp(fogY,view.yMin,view.yMax-view.height*.16f);}
   var a=GuideLocal(view.min,root);var b=GuideLocal(view.max,root);shade.opening=UnityEngine.Rect.MinMaxRect(a.x,a.y,b.x,b.y);shade.SetVerticesDirty();focus.sizeDelta=b-a;focus.anchoredPosition=(a+b)*.5f;
   var low=GuideLocal(CurrentSafeArea.min,root);var high=GuideLocal(CurrentSafeArea.max,root);float width=Mathf.Min(860,high.x-low.x-64);float middle=Mathf.Clamp(a.y-height*.5f-42,low.y+height*.5f+140,high.y-height*.5f-24);
   card.sizeDelta=new Vector2(width,height);card.anchoredPosition=new Vector2((low.x+high.x)*.5f,middle);title.rectTransform.sizeDelta=new Vector2(width-56,60);copy.rectTransform.sizeDelta=new Vector2(width-72,152);br.sizeDelta=new Vector2(Mathf.Min(400,width-88),84);
   var target=root.InverseTransformPoint(br.TransformPoint(new Vector3(br.rect.center.x,br.rect.yMin-18,0)));hand.anchoredPosition=target;pulse.handOrigin=target;
  };RefreshGameplayRule();
 }
 Vector2 GuideLocal(Vector2 screen,RectTransform root){RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,GuideCamera,out var p);return p;}
}}
