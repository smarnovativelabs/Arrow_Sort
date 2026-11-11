using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed class WoolDifficultyAnnouncement:MonoBehaviour {
 RectTransform inputBlocker;
 RectTransform ribbon;TMP_Text title,subtitle;Image duckBadge;Coroutine motion;
 public bool IsVisible=>ribbon&&ribbon.gameObject.activeSelf;
 public void Show(int level){
  var difficulty=WoolEndlessGenerator.DifficultyFor(level);
  if(motion!=null)StopCoroutine(motion);
  if(difficulty<WoolDifficulty.Hard){if(ribbon)ribbon.gameObject.SetActive(false);SetInputBlocked(false);return;}
  SetInputBlocked(true);
  if(!ribbon){
   ribbon=new GameObject("Duck rescue difficulty banner",typeof(RectTransform),typeof(CanvasRenderer),typeof(WoolDifficultyRibbon)).GetComponent<RectTransform>();ribbon.SetParent(transform,false);ribbon.GetComponent<WoolDifficultyRibbon>().raycastTarget=false;
   title=Label("Difficulty title",new Vector2(120,10),new Vector2(-20,-10),56);
   subtitle=Label("Difficulty subtitle",new Vector2(120,8),new Vector2(-20,-100),27);
   title.rectTransform.offsetMin=new Vector2(172,66);title.rectTransform.offsetMax=new Vector2(-20,-10);subtitle.rectTransform.offsetMin=new Vector2(172,8);
   var mask=new GameObject("Round duck medallion",typeof(RectTransform),typeof(CanvasRenderer),typeof(WoolDifficultyBadgeClip),typeof(Mask));mask.transform.SetParent(ribbon,false);mask.GetComponent<Mask>().showMaskGraphic=false;mask.GetComponent<WoolDifficultyBadgeClip>().raycastTarget=false;
   var maskRect=mask.GetComponent<RectTransform>();maskRect.anchorMin=maskRect.anchorMax=new Vector2(0,.5f);maskRect.anchoredPosition=new Vector2(92,0);maskRect.sizeDelta=new Vector2(140,140);
   var badge=new GameObject("Duck rescue badge",typeof(RectTransform),typeof(Image));badge.transform.SetParent(mask.transform,false);duckBadge=badge.GetComponent<Image>();duckBadge.raycastTarget=false;duckBadge.preserveAspect=true;
   var badgeRect=duckBadge.rectTransform;badgeRect.anchorMin=badgeRect.anchorMax=new Vector2(.5f,.5f);badgeRect.anchoredPosition=Vector2.zero;badgeRect.sizeDelta=new Vector2(140,140);
   var spin=GetComponentInChildren<WoolLuckySpin>(true);duckBadge.sprite=spin?(spin.referenceHub?spin.referenceHub:spin.referenceDuck):null;duckBadge.enabled=duckBadge.sprite;
  }
  ribbon.GetComponent<WoolDifficultyRibbon>().superHard=difficulty==WoolDifficulty.SuperHard;
  ribbon.GetComponent<WoolDifficultyRibbon>().SetVerticesDirty();
  title.text=difficulty==WoolDifficulty.SuperHard?"SUPER HARD":"HARD LEVEL";
  subtitle.text="LEVEL "+level+"  •  Rescue the duck family";
  ribbon.gameObject.SetActive(true);ribbon.SetAsLastSibling();motion=StartCoroutine(Slide());
 }
 void SetInputBlocked(bool blocked){
  if(blocked&&!inputBlocker){
   var canvas=GetComponentInParent<Canvas>();
   var root=canvas?canvas.rootCanvas.transform:transform;
   var panel=new GameObject("Difficulty announcement input blocker",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster),typeof(Image),typeof(Button));
   inputBlocker=panel.GetComponent<RectTransform>();inputBlocker.SetParent(root,false);
   inputBlocker.anchorMin=Vector2.zero;inputBlocker.anchorMax=Vector2.one;inputBlocker.offsetMin=inputBlocker.offsetMax=Vector2.zero;
   var overlay=panel.GetComponent<Canvas>();overlay.overrideSorting=true;overlay.sortingOrder=32760;if(canvas)overlay.sortingLayerID=canvas.sortingLayerID;
   var image=panel.GetComponent<Image>();image.color=Color.clear;image.raycastTarget=true;
   var button=panel.GetComponent<Button>();button.transition=Selectable.Transition.None;button.navigation=new Navigation{mode=Navigation.Mode.None};
  }
  if(inputBlocker){inputBlocker.gameObject.SetActive(blocked);if(blocked)inputBlocker.SetAsLastSibling();}
 }
 TMP_Text Label(string name,Vector2 min,Vector2 max,float size){var rt=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<RectTransform>();rt.SetParent(ribbon,false);rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=min;rt.offsetMax=max;var text=rt.GetComponent<TextMeshProUGUI>();text.font=TMP_Settings.defaultFontAsset;foreach(var source in GetComponentsInChildren<TMP_Text>(true))if(source.font&&source.font.name.Contains("Lilita")){text.font=source.font;break;}text.fontSize=size;text.enableAutoSizing=true;text.fontSizeMin=size*.65f;text.fontSizeMax=size;text.alignment=TextAlignmentOptions.Center;text.color=new Color(1,.98f,.85f);text.outlineColor=new Color(.28f,.10f,.06f);text.outlineWidth=.17f;text.raycastTarget=false;return text;}
 IEnumerator Slide(){
  var parent=(RectTransform)transform;float width=Mathf.Min(parent.rect.width*.92f,980);ribbon.sizeDelta=new Vector2(width,width*.24f);var rig=FindFirstObjectByType<WoolSceneRig>();ribbon.anchorMin=ribbon.anchorMax=new Vector2(.5f,rig&&rig.SlotCamera?rig.SlotCamera.rect.yMin:.55f);ribbon.localScale=Vector3.one;ribbon.localRotation=Quaternion.identity;
  float travel=(parent.rect.width+width)*.6f;
  for(float t=0;t<.3f;t+=Time.unscaledDeltaTime){ribbon.anchoredPosition=new Vector2(Mathf.Lerp(-travel,0,Mathf.SmoothStep(0,1,t/.3f)),0);yield return null;}
  ribbon.anchoredPosition=Vector2.zero;yield return new WaitForSecondsRealtime(2);
  for(float t=0;t<.3f;t+=Time.unscaledDeltaTime){ribbon.anchoredPosition=new Vector2(Mathf.Lerp(0,travel,Mathf.SmoothStep(0,1,t/.3f)),0);yield return null;}
  ribbon.gameObject.SetActive(false);SetInputBlocked(false);motion=null;
 }
 void OnDisable(){if(motion!=null)StopCoroutine(motion);motion=null;if(ribbon)ribbon.gameObject.SetActive(false);SetInputBlocked(false);}
 void OnDestroy(){if(inputBlocker)Destroy(inputBlocker.gameObject);}
}
public sealed class WoolDifficultyBadgeClip:MaskableGraphic {
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();float r=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.43f;vh.AddVert(Vector2.zero,Color.white,Vector2.zero);for(int i=0;i<=48;i++){float a=i*Mathf.PI/24;vh.AddVert(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,Color.white,Vector2.zero);if(i>0)vh.AddTriangle(0,i,i+1);}}
}
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolDifficultyRibbon:MaskableGraphic {
 public bool superHard;
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();raycastTarget=false;float w=rectTransform.rect.width,h=rectTransform.rect.height;
  Color top=superHard?new Color(.57f,.20f,.79f):new Color(1,.31f,.12f);
  Color bottom=superHard?new Color(.23f,.065f,.43f):new Color(.60f,.075f,.045f);
  Rounded(vh,new Vector2(0,-h*.055f),w,h,h*.22f,new Color(.08f,.025f,.10f,.38f),new Color(.08f,.025f,.10f,.38f));
  Rounded(vh,Vector2.zero,w,h,h*.22f,new Color(1,.94f,.54f),new Color(.75f,.38f,.07f));
  Rounded(vh,new Vector2(0,1),w-10,h-12,h*.19f,new Color(1,.68f,.16f),new Color(1,.88f,.43f));
  Rounded(vh,Vector2.zero,w-22,h-25,h*.17f,top,bottom);
  Rounded(vh,new Vector2(0,h*.225f),w-42,h*.36f,h*.10f,new Color(1,1,1,.20f),new Color(1,1,1,.015f));
  // Embossed medallion behind the illustrated duck badge.
  var c=new Vector2(-w*.5f+92,-2);
  Disc(vh,c+Vector2.down*5,h*.37f,new Color(.15f,.04f,.10f,.35f));
  Disc(vh,c,h*.37f,new Color(1,.87f,.35f));
  Disc(vh,c,h*.32f,new Color(.78f,.41f,.09f));
  Disc(vh,c+Vector2.up*2,h*.29f,new Color(1,.95f,.67f));
  Star(vh,new Vector2(w*.43f,h*.27f),h*.075f,new Color(1,.96f,.66f,.9f));
  Star(vh,new Vector2(w*.39f,-h*.29f),h*.045f,new Color(1,.86f,.36f,.7f));
 }
 static void Rounded(VertexHelper vh,Vector2 center,float width,float height,float radius,Color top,Color bottom){
  int first=vh.currentVertCount;vh.AddVert(center,Color.Lerp(bottom,top,.5f),Vector2.zero);
  for(int corner=0;corner<4;corner++)for(int step=0;step<=8;step++){
   float angle=(corner*90+step*90f/8)*Mathf.Deg2Rad;
   var origin=new Vector2(corner==0||corner==3?width*.5f-radius:-width*.5f+radius,corner<2?height*.5f-radius:-height*.5f+radius);
   var p=center+origin+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
   vh.AddVert(p,Color.Lerp(bottom,top,(p.y-center.y)/height+.5f),Vector2.zero);
  }
  int count=vh.currentVertCount-first-1;for(int i=0;i<count;i++)vh.AddTriangle(first,first+1+i,first+1+(i+1)%count);
 }
 static void Star(VertexHelper vh,Vector2 c,float r,Color color){Quad(vh,c+Vector2.up*r,c+Vector2.right*r*.25f,c+Vector2.down*r,c+Vector2.left*r*.25f,color);Quad(vh,c+Vector2.right*r,c+Vector2.down*r*.25f,c+Vector2.left*r,c+Vector2.up*r*.25f,color);}
 static void Quad(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color){int n=v.currentVertCount;v.AddVert(a,color,Vector2.zero);v.AddVert(b,color,Vector2.zero);v.AddVert(c,color,Vector2.zero);v.AddVert(d,color,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
 static void Disc(VertexHelper v,Vector2 center,float radius,Color color){int n=v.currentVertCount;v.AddVert(center,color,Vector2.zero);for(int i=0;i<=32;i++){float a=i*Mathf.PI/16;v.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);if(i>0)v.AddTriangle(n,n+i,n+i+1);}}
}
}
