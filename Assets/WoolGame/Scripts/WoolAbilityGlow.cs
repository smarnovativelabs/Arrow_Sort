using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolAbilityGlow:MaskableGraphic {
 [SerializeField] Color shineColor=Color.white;
 [SerializeField,Range(0,1)] float shineOpacity=.46f;
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();var r=rectTransform.rect;
  for(int i=0;i<16;i++){float a=i*Mathf.PI*2/16,b=a+.16f;Add(vh,r,a,b,0,.65f,Tint(.10f),Tint(shineOpacity));Add(vh,r,a,b,.65f,1,Tint(shineOpacity),Tint(0));}
 }
 Color Tint(float alpha){return new Color(shineColor.r,shineColor.g,shineColor.b,alpha*shineColor.a);}
 void Add(VertexHelper vh,Rect r,float a,float b,float inner,float outer,Color ci,Color co){int n=vh.currentVertCount;vh.AddVert(P(r,a,inner),ci,Vector2.zero);vh.AddVert(P(r,b,inner),ci,Vector2.zero);vh.AddVert(P(r,b,outer),co,Vector2.zero);vh.AddVert(P(r,a,outer),co,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
 Vector3 P(Rect r,float a,float k){return new Vector3(Mathf.Cos(a)*r.width*.5f*k,Mathf.Sin(a)*r.height*.5f*k,0);}
}}