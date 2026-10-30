using UnityEngine;
namespace WoolGame { public sealed partial class WoolHud {

 public float AbilityStripTopPixels {
  get {
   if(!footer||!footer.gameObject.activeInHierarchy||!canvas)return 0;
   var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform,footer);
   return RectTransformUtility.WorldToScreenPoint(null,canvas.transform.TransformPoint(bounds.max)).y;
  }
 }

 readonly Vector3[] mobileCorners=new Vector3[4];
 void LateUpdate(){ClampMobileControls();RefreshTutorialOverlays();RefreshGameplayRule();SetDanger(requestedDangerIntensity,requestedDangerTime);}
 internal void ClampMobileControls(){
 if(header&&header.gameObject.activeInHierarchy&&canvas)foreach(string name in new[]{"Pause","Level","Gameplay speed"}){var rt=header.Find(name) as RectTransform;if(rt&&rt.gameObject.activeInHierarchy)ClampHeaderControl(rt);}

 if(!footer||!footer.gameObject.activeInHierarchy||!canvas)return;
 var area=CurrentSafeArea;var parent=footer.parent as RectTransform;if(!parent)return;
 var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(parent,footer);
 var low=RectTransformUtility.WorldToScreenPoint(null,parent.TransformPoint(bounds.min));
 var high=RectTransformUtility.WorldToScreenPoint(null,parent.TransformPoint(bounds.max));
 float dy=Mathf.Max(0,area.yMin+28-low.y),dx=0;
 if(low.x<area.xMin+16)dx=area.xMin+16-low.x;
 if(high.x+dx>area.xMax-16)dx+=area.xMax-16-(high.x+dx);
 if(Mathf.Abs(dx)+Mathf.Abs(dy)<.5f)return;
 Vector2 from,to;RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,Vector2.zero,null,out from);
 RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,new Vector2(dx,dy),null,out to);
 footer.anchoredPosition+=to-from;
 }
 void ClampHeaderControl(RectTransform control){
 var parent=control.parent as RectTransform;if(!parent)return;control.GetWorldCorners(mobileCorners);var min=Vector2.one*float.MaxValue;var max=Vector2.one*float.MinValue;foreach(var corner in mobileCorners){var p=RectTransformUtility.WorldToScreenPoint(null,corner);min=Vector2.Min(min,p);max=Vector2.Max(max,p);}var area=CurrentSafeArea;float dx=min.x<area.xMin+16?area.xMin+16-min.x:max.x>area.xMax-16?area.xMax-16-max.x:0;float dy=min.y<area.yMin+16?area.yMin+16-min.y:max.y>area.yMax-16?area.yMax-16-max.y:0;if(Mathf.Abs(dx)+Mathf.Abs(dy)<.5f)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,Vector2.zero,null,out var from);RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,new Vector2(dx,dy),null,out var to);control.anchoredPosition+=to-from;
 }
} }
