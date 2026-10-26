using UnityEngine;using UnityEngine.UI;
namespace WoolGame {
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolNamePencilGraphic:MaskableGraphic {
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();Disc(v,.49f,new Color(1,.48f,.02f));Disc(v,.43f,new Color(1,.81f,.12f));Quad(v,new Vector2(.28f,.38f),new Vector2(.60f,.70f),new Vector2(.72f,.58f),new Vector2(.40f,.26f),Color.white);Tri(v,new Vector2(.25f,.21f),new Vector2(.28f,.38f),new Vector2(.40f,.26f),new Color(.45f,.21f,.02f));Quad(v,new Vector2(.63f,.73f),new Vector2(.69f,.79f),new Vector2(.81f,.67f),new Vector2(.75f,.61f),Color.white);}
 Vector3 P(Vector2 p){var r=rectTransform.rect;return new Vector3(r.xMin+r.width*p.x,r.yMin+r.height*p.y);}
 void Disc(VertexHelper v,float radius,Color c){int n=v.currentVertCount;v.AddVert(P(new Vector2(.5f,.5f)),c,Vector2.zero);for(int i=0;i<49;i++){float a=i*Mathf.PI*2/48;v.AddVert(P(new Vector2(.5f+Mathf.Cos(a)*radius,.5f+Mathf.Sin(a)*radius)),c,Vector2.zero);if(i>0)v.AddTriangle(n,n+i,n+i+1);}}
 void Tri(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Color color){int n=v.currentVertCount;v.AddVert(P(a),color,Vector2.zero);v.AddVert(P(b),color,Vector2.zero);v.AddVert(P(c),color,Vector2.zero);v.AddTriangle(n,n+1,n+2);}
 void Quad(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color){int n=v.currentVertCount;v.AddVert(P(a),color,Vector2.zero);v.AddVert(P(b),color,Vector2.zero);v.AddVert(P(c),color,Vector2.zero);v.AddVert(P(d),color,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
}}
