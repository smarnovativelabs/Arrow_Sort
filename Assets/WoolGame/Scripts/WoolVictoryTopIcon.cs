using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolVictoryTopIcon:MaskableGraphic {
 public bool home;
 void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d){int n=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(c,color,Vector2.zero);vh.AddVert(d,color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();float s=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
  if(home){
   Quad(vh,new Vector2(-s*.82f,0),new Vector2(0,s*.78f),new Vector2(s*.82f,0),new Vector2(0,s*.26f));
   Quad(vh,new Vector2(-s*.57f,-s*.65f),new Vector2(-s*.57f,s*.12f),new Vector2(-s*.14f,s*.12f),new Vector2(-s*.14f,-s*.65f));
   Quad(vh,new Vector2(s*.14f,-s*.65f),new Vector2(s*.14f,s*.12f),new Vector2(s*.57f,s*.12f),new Vector2(s*.57f,-s*.65f));
   Quad(vh,new Vector2(-s*.16f,-s*.14f),new Vector2(-s*.16f,s*.28f),new Vector2(s*.16f,s*.28f),new Vector2(s*.16f,-s*.14f));
  }else{
   for(int i=0;i<2;i++){float angle=(i==0?45:-45)*Mathf.Deg2Rad;Vector2 d=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*s*.82f,p=new Vector2(-Mathf.Sin(angle),Mathf.Cos(angle))*s*.14f;Quad(vh,-d-p,-d+p,d+p,d-p);}
  }
 }
}}
