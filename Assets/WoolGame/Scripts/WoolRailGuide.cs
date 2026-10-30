using System;
using TMPro;
using UnityEngine;
namespace WoolGame {
public sealed class WoolRailGuide:MonoBehaviour {
 public RectTransform bubble;public TMP_Text label;public WoolRailGuideTail tail;
 WoolVehicleSources source;Camera view;Action finished;float elapsed;bool completing;
 public void RefreshCopy(){label.text=WoolWarehouseTutorial.TunnelExplanation;label.fontSize=36;label.textWrappingMode=TextWrappingModes.Normal;label.rectTransform.sizeDelta=new Vector2(640,180);bubble.sizeDelta=new Vector2(670,215);((RectTransform)label.transform.parent).sizeDelta=new Vector2(656,201);}
 public void Open(WoolVehicleSources target,Camera camera,Action complete){source=target;view=camera;finished=complete;elapsed=WoolProgress.RailTutorialElapsed;completing=false;RefreshCopy();gameObject.SetActive(true);transform.SetAsLastSibling();Position();}
 public void Close(){gameObject.SetActive(false);}
 void OnDisable(){if(Application.isPlaying&&!completing&&source)WoolProgress.SaveRailTutorialElapsed(elapsed);}
 void LateUpdate(){if(!source||!view){Close();return;}var hud=GetComponentInParent<WoolHud>();if(hud&&(hud.ModalVisible||hud.AdVisible))return;Position();elapsed+=Mathf.Min(Time.unscaledDeltaTime,.1f);if(elapsed>=6){completing=true;finished?.Invoke();Close();}}
 void Position(){var root=(RectTransform)transform;Vector2 target;RectTransformUtility.ScreenPointToLocalPointInRectangle(root,view.WorldToScreenPoint(source.RailEntryWorld),null,out target);float half=bubble.rect.width*.5f;var r=root.rect;float y=target.y+190;bubble.anchoredPosition=new Vector2(Mathf.Clamp(target.x-130,r.xMin+half+20,r.xMax-half-20),Mathf.Clamp(y,r.yMin+100,r.yMax-100));tail.tip=target;tail.SetVerticesDirty();}
}
public sealed partial class WoolHud {
 [SerializeField] WoolRailGuide railGuide;
 void OpenRailGuide(){if(!railGuide||WoolProgress.RailTutorialSeen)return;var source=FindFirstObjectByType<WoolVehicleSources>();var rig=FindFirstObjectByType<WoolSceneRig>();if(source&&rig)railGuide.Open(source,rig.vehicleCamera,()=>WoolProgress.MarkRailTutorialSeen());}
#if UNITY_EDITOR
 public void AuthorRailGuide(){
  if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required");
  if(railGuide){if(!railGuide.tail.GetComponent<CanvasRenderer>())railGuide.tail.gameObject.AddComponent<CanvasRenderer>();railGuide.RefreshCopy();return;}
  var root=Rect("Rail tunnel message",safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);railGuide=root.gameObject.AddComponent<WoolRailGuide>();
  var group=root.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
  var tailRoot=Rect("Speech pointer",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);tailRoot.gameObject.AddComponent<CanvasRenderer>();railGuide.tail=tailRoot.gameObject.AddComponent<WoolRailGuideTail>();railGuide.tail.raycastTarget=false;
  var box=Panel("Refill message",root,new Vector2(.5f,.5f),new Vector2(330,110),Vector2.zero,WoolArt.Hex("FFC348"));box.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
  var inner=Panel("Message face",box,new Vector2(.5f,.5f),new Vector2(316,96),Vector2.zero,WoolArt.Hex("FFF4D9"));inner.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
  railGuide.bubble=box;railGuide.tail.bubble=box;
  railGuide.label=Label("Short rail guide",inner,"REFILL",52,new Vector2(290,90),Vector2.zero,WoolArt.Hex("61310F"));railGuide.label.raycastTarget=false;railGuide.label.alignment=TextAlignmentOptions.Center;railGuide.label.enableAutoSizing=false;
  railGuide.RefreshCopy();root.gameObject.SetActive(false);UnityEditor.EditorUtility.SetDirty(this);
 }
#endif
}
}
