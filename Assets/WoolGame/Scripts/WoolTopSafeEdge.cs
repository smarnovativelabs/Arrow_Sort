using UnityEngine;
namespace WoolGame{
[DefaultExecutionOrder(300)]
public sealed class WoolTopSafeEdge:MonoBehaviour{
 readonly Vector3[] corners=new Vector3[4];RectTransform rect;Canvas canvas;
 void LateUpdate(){if(!rect)rect=transform as RectTransform;if(!canvas)canvas=GetComponentInParent<Canvas>();if(!rect||!canvas||!(rect.parent is RectTransform parent))return;var camera=canvas.rootCanvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.rootCanvas.worldCamera;rect.GetWorldCorners(corners);float top=float.MinValue;foreach(var corner in corners)top=Mathf.Max(top,RectTransformUtility.WorldToScreenPoint(camera,corner).y);float delta=WoolHud.CurrentSafeArea.yMax-6-top;if(Mathf.Abs(delta)<.25f)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,Vector2.zero,camera,out var from);RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,new Vector2(0,delta),camera,out var to);rect.anchoredPosition+=to-from;}
}}
