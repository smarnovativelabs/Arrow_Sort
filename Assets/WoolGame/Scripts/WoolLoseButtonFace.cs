using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
public sealed class WoolLoseButtonFace:MaskableGraphic {
 public Color top=new Color(1,.81f,.12f),bottom=new Color(1,.43f,.015f);
 protected override void OnPopulateMesh(VertexHelper vh){
 vh.Clear();var r=rectTransform.rect;float radius=Mathf.Min(r.height*.46f,r.width*.2f);
 var points=new System.Collections.Generic.List<Vector2>(40);
 for(int corner=0;corner<4;corner++){
  float cx=corner<2?r.xMax-radius:r.xMin+radius;
  float cy=corner==0||corner==3?r.yMax-radius:r.yMin+radius;
  float start=90-corner*90;
  for(int j=0;j<=10;j++){float a=(start-j*9)*Mathf.Deg2Rad;points.Add(new Vector2(cx+Mathf.Cos(a)*radius,cy+Mathf.Sin(a)*radius));}
 }
 // Clockwise rounded perimeter: top right -> bottom right -> bottom left -> top left.
 Add(vh,r.center,r);foreach(var p in points)Add(vh,p,r);
 for(int i=0;i<points.Count;i++)vh.AddTriangle(0,i+1,(i+1)%points.Count+1);
 }
 void Add(VertexHelper vh,Vector2 p,Rect r){
 float t=Mathf.InverseLerp(r.yMin,r.yMax,p.y);var c=Color.Lerp(bottom,top,t);float gloss=Mathf.Exp(-Mathf.Pow((t-.78f)*7,2))*.20f;c=Color.Lerp(c,Color.white,gloss);c.a*=color.a;vh.AddVert(p,c,new Vector2(Mathf.InverseLerp(r.xMin,r.xMax,p.x),t));
 }
}}
