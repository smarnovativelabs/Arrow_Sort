using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed partial class WoolHud {
 RectTransform removalShade;Button removalCancel;TMPro.TMP_Text removalCopy;RectTransform removalBanner;
 public void RemovalSpotlight(bool show){
  if(!removalShade&&show){
   removalShade=Rect("Removal selection screen shade",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
   var graphic=removalShade.gameObject.AddComponent<WoolAbilitySpotlight>();graphic.raycastTarget=true;graphic.passThroughOpening=true;
  }
  if(!removalShade)return;
  removalShade.gameObject.SetActive(show);if(!show)return;
  removalShade.SetAsLastSibling();removalShade.anchorMin=Vector2.zero;removalShade.anchorMax=Vector2.one;removalShade.offsetMin=removalShade.offsetMax=Vector2.zero;
  Canvas.ForceUpdateCanvases();
  var cut=removalShade.GetComponent<WoolAbilitySpotlight>();cut.color=new Color(0,0,0,.42f);
  var rig=FindFirstObjectByType<WoolSceneRig>();var camera=rig.vehicleCamera;var pixels=camera.pixelRect;
  var footerCorners=new Vector3[4];footer.GetWorldCorners(footerCorners);
  float floor=RectTransformUtility.WorldToScreenPoint(GuideCamera,footerCorners[1]).y+12;
  float ceiling=camera.WorldToScreenPoint(rig.slots[0].position).y-65*canvas.scaleFactor;
  float lower=Mathf.Max(pixels.yMin,floor),upper=Mathf.Min(pixels.yMax,ceiling);
  if(upper>lower+32){pixels.yMin=lower;pixels.yMax=upper;}
  RectTransformUtility.ScreenPointToLocalPointInRectangle(removalShade,pixels.min,GuideCamera,out var min);
  RectTransformUtility.ScreenPointToLocalPointInRectangle(removalShade,pixels.max,GuideCamera,out var max);
  cut.opening=UnityEngine.Rect.MinMaxRect(min.x,min.y,max.x,max.y);cut.SetVerticesDirty();
  if(removalInstruction)removalInstruction.gameObject.SetActive(false);
  float width=Mathf.Min(1000,removalShade.rect.width-32);
  if(!removalBanner){
   removalBanner=Panel("Removal instruction banner",removalShade,new Vector2(.5f,.5f),new Vector2(width,88),Vector2.zero,WoolArt.Hex("F5B62E"));
   removalBanner.GetComponent<Image>().raycastTarget=false;
   removalCopy=Label("Choose a vehicle",removalBanner,"Remove any vehicle",40,new Vector2(width-32,72),Vector2.zero,WoolArt.Hex("59361E"));
  }
  removalBanner.sizeDelta=new Vector2(width,88);removalBanner.anchoredPosition=new Vector2(0,Mathf.Min(max.y+48,removalShade.rect.yMax-56));
  removalCopy.rectTransform.sizeDelta=new Vector2(width-32,72);removalCopy.enableAutoSizing=true;removalCopy.fontSizeMin=24;removalCopy.fontSizeMax=40;
  if(!removalCancel){
   removalCancel=ButtonAt("Cancel removal choice",removalShade,"CANCEL",new Vector2(320,92),Vector2.zero,null);
   var image=removalCancel.GetComponent<Image>();if(skin!=null&&skin.Length>2)image.sprite=skin[2];image.color=Color.white;
   var layer=removalCancel.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=400;
   removalCancel.gameObject.AddComponent<GraphicRaycaster>();
  }
  removalCancel.onClick.RemoveAllListeners();removalCancel.onClick.AddListener(()=>cancelSelection.onClick.Invoke());
  var r=(RectTransform)removalCancel.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,0);r.pivot=new Vector2(.5f,0);
  var safeMin=Vector2.zero;RectTransformUtility.ScreenPointToLocalPointInRectangle(removalShade,CurrentSafeArea.min,GuideCamera,out safeMin);
  r.anchoredPosition=new Vector2(0,Mathf.Max(24,safeMin.y-removalShade.rect.yMin+24));r.sizeDelta=new Vector2(320,92);r.localScale=Vector3.one;
  var cancelText=removalCancel.GetComponentInChildren<TMPro.TMP_Text>(true);cancelText.gameObject.SetActive(true);cancelText.text="CANCEL";
  cancelText.rectTransform.anchorMin=cancelText.rectTransform.anchorMax=new Vector2(.5f,.5f);cancelText.rectTransform.anchoredPosition=Vector2.zero;
  cancelText.rectTransform.pivot=new Vector2(.5f,.5f);cancelText.rectTransform.localScale=Vector3.one;cancelText.rectTransform.localRotation=Quaternion.identity;
  cancelText.alignment=TMPro.TextAlignmentOptions.Center;cancelText.margin=Vector4.zero;
  cancelText.textWrappingMode=TMPro.TextWrappingModes.NoWrap;
  cancelText.rectTransform.sizeDelta=new Vector2(280,72);cancelText.fontSizeMax=38;cancelText.fontSizeMin=24;cancelText.enableAutoSizing=true;cancelText.color=Color.white;
  removalCancel.interactable=true;removalCancel.gameObject.SetActive(true);cancelSelection.gameObject.SetActive(false);
 }
 public IEnumerator PlayRemovalVolley(Vector2 origin,List<Vector2> destinations,Color arrowColor){
  var root=Rect("Removal arrows to matching colour",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);root.SetAsLastSibling();root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
  RectTransformUtility.ScreenPointToLocalPointInRectangle(root,origin,GuideCamera,out var from);var ends=new List<Vector2>();var arrows=new List<RectTransform>();
  for(int i=0;i<destinations.Count;i++){RectTransformUtility.ScreenPointToLocalPointInRectangle(root,destinations[i],GuideCamera,out var to);ends.Add(to);var arrow=Rect("Colour clearing arrow "+i,root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(24,75),from);arrow.gameObject.AddComponent<CanvasRenderer>();var graphic=arrow.gameObject.AddComponent<WoolWarehouseArrow>();graphic.color=arrowColor;graphic.raycastTarget=false;arrows.Add(arrow);}
  try{
   float duration=.65f+Mathf.Max(0,destinations.Count-1)*.065f;
   for(float time=0;time<duration;time+=Time.unscaledDeltaTime){for(int i=0;i<arrows.Count;i++){float t=Mathf.Clamp01((time-i*.065f)/.65f);arrows[i].gameObject.SetActive(t>0&&t<1);var mid=(from+ends[i])*.5f+new Vector2((i-destinations.Count*.5f)*25,110);var a=Vector2.Lerp(from,mid,t);var b=Vector2.Lerp(mid,ends[i],t);arrows[i].anchoredPosition=Vector2.Lerp(a,b,t);var tangent=b-a;arrows[i].localRotation=Quaternion.Euler(0,0,Mathf.Atan2(tangent.y,tangent.x)*Mathf.Rad2Deg-90);}yield return null;}
  }finally{if(root)Destroy(root.gameObject);}
 }
}
}
