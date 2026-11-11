using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
 [DefaultExecutionOrder(400)]
 public sealed class WoolLockedSlotOverlay:MonoBehaviour
 {
  WoolSceneRig rig;RectTransform root;WoolSlotPlusGraphic[] icons;TMP_Text[] labels;Renderer[] nativeLabels;Material labelMaterial;
  void LateUpdate(){
   if(!rig)rig=GetComponent<WoolSceneRig>();if(!rig||!rig.hud||!rig.SlotCamera)return;
   if(!root){
    root=new GameObject("Slot labels and clean plus signs",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(rig.hud.transform,false);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
    icons=new WoolSlotPlusGraphic[rig.slots.Length];labels=new TMP_Text[rig.slots.Length];nativeLabels=new Renderer[rig.slots.Length];
    labelMaterial=new Material(rig.counts[0].font.material);labelMaterial.SetColor(ShaderUtilities.ID_FaceColor,Color.white);labelMaterial.SetColor(ShaderUtilities.ID_OutlineColor,WoolArt.Hex("27313E"));labelMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth,.22f);labelMaterial.EnableKeyword("OUTLINE_ON");
    for(int i=0;i<icons.Length;i++){
     nativeLabels[i]=rig.counts[i].GetComponent<Renderer>();
     var icon=new GameObject("Slot "+(i+1)+" simple plus",typeof(RectTransform),typeof(CanvasRenderer),typeof(WoolSlotPlusGraphic));icon.transform.SetParent(root,false);icons[i]=icon.GetComponent<WoolSlotPlusGraphic>();icons[i].raycastTarget=false;
     var label=new GameObject("Slot caption "+i,typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(root,false);labels[i]=label.GetComponent<TMP_Text>();labels[i].font=rig.counts[i].font;labels[i].fontSharedMaterial=labelMaterial;labels[i].alignment=TextAlignmentOptions.Center;labels[i].fontStyle=FontStyles.Bold;labels[i].color=Color.white;labels[i].raycastTarget=false;labels[i].textWrappingMode=TextWrappingModes.NoWrap;
    }
    // Consecutive symbols and shared-material captions can batch independently.
    foreach(var label in labels)label.transform.SetAsLastSibling();
   }
   root.SetAsFirstSibling();
   var canvas=root.GetComponentInParent<Canvas>().rootCanvas;var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;float units=root.rect.width/Mathf.Max(1,Screen.width);
   bool gameplay=rig.hud.GameplayControlsVisible&&!rig.hud.AdVisible&&rig.world.gameObject.activeInHierarchy;
   for(int i=0;i<icons.Length;i++){
    rig.counts[i].color=new Color(1,1,1,0);
    if(nativeLabels[i]&&nativeLabels[i].enabled)nativeLabels[i].enabled=false;
    bool visible=gameplay&&!string.IsNullOrEmpty(rig.counts[i].text);bool locked=visible&&(rig.counts[i].text=="Unlock"||rig.counts[i].text=="AD +"||rig.counts[i].text=="+");
    icons[i].gameObject.SetActive(locked);labels[i].gameObject.SetActive(visible);if(!visible)continue;
    var screen=rig.SlotCamera.WorldToScreenPoint(rig.slots[i].position);int neighbor=i==rig.slots.Length-1?i-1:i+1;float spacing=Mathf.Abs(rig.SlotCamera.WorldToScreenPoint(rig.slots[neighbor].position).x-screen.x);
    RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,camera,out var point);
    var icon=icons[i].rectTransform;icon.anchorMin=icon.anchorMax=icon.pivot=Vector2.one*.5f;icon.anchoredPosition=point;icon.sizeDelta=Vector2.one*spacing*.62f*units;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(root,new Vector2(screen.x,screen.y-spacing*.42f),camera,out var captionPoint);
    var label=labels[i].rectTransform;label.anchorMin=label.anchorMax=label.pivot=Vector2.one*.5f;label.anchoredPosition=captionPoint;label.sizeDelta=new Vector2(spacing*.94f,spacing*.30f)*units;
    labels[i].text=locked?"Unlock":rig.counts[i].text;labels[i].fontSize=spacing*.24f*units;
    rig.counts[i].color=new Color(1,1,1,0);if(rig.slotUnlockIcons[i])rig.slotUnlockIcons[i].SetActive(false);
   }
  }
  void OnDestroy(){if(root)Destroy(root.gameObject);if(labelMaterial)Destroy(labelMaterial);}
 }
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class WoolSlotPlusGraphic:MaskableGraphic
 {
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();float s=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height);ButtonDisc(vh,s*.5f,new Color(.47f,.88f,.16f),new Color(.14f,.47f,.035f));ButtonDisc(vh,s*.445f,new Color(.58f,.96f,.18f),new Color(.20f,.66f,.035f));float r=s*.29f,t=s*.085f;Cross(vh,r-t,t,Color.white);Disc(vh,new Vector2(0,r-t),t);Disc(vh,new Vector2(0,-r+t),t);Disc(vh,new Vector2(r-t,0),t);Disc(vh,new Vector2(-r+t,0),t);}
  static void ButtonDisc(VertexHelper vh,float radius,Color top,Color bottom){int n=vh.currentVertCount;vh.AddVert(Vector2.zero,Color.Lerp(bottom,top,.5f),Vector2.zero);for(int i=0;i<=64;i++){float a=i*Mathf.PI/32;var p=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;vh.AddVert(p,Color.Lerp(bottom,top,p.y/(radius*2)+.5f),Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}}
  static void Disc(VertexHelper vh,Vector2 center,float radius){int n=vh.currentVertCount;vh.AddVert(center,Color.white,Vector2.zero);for(int i=0;i<=32;i++){float a=i*Mathf.PI/16;vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,Color.white,Vector2.zero);if(i>0)vh.AddTriangle(n,n+i,n+i+1);}}
  static void Cross(VertexHelper vh,float radius,float halfWidth,Color color){Quad(vh,new Vector2(-halfWidth,-radius),new Vector2(halfWidth,radius),color);Quad(vh,new Vector2(-radius,-halfWidth),new Vector2(radius,halfWidth),color);}
  static void Quad(VertexHelper vh,Vector2 min,Vector2 max,Color color){int n=vh.currentVertCount;vh.AddVert(min,color,Vector2.zero);vh.AddVert(new Vector2(min.x,max.y),color,Vector2.zero);vh.AddVert(max,color,Vector2.zero);vh.AddVert(new Vector2(max.x,min.y),color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
 }
}

