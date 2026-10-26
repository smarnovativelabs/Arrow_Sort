using UnityEngine;using UnityEngine.UI;
namespace WoolGame {
public sealed class WoolRewardVideoGraphic:MaskableGraphic {
protected override void OnPopulateMesh(VertexHelper v){v.Clear();Quad(v,.09f,.12f,.92f,.72f,new Color(.65f,.07f,.015f));Quad(v,.14f,.17f,.88f,.70f,new Color(1,.18f,.04f));Quad(v,.08f,.76f,.92f,.98f,new Color(.95f,.1f,.015f));for(int i=0;i<3;i++)Poly(v,new Vector2(.14f+i*.25f,.76f),new Vector2(.28f+i*.25f,.76f),new Vector2(.37f+i*.25f,.98f),new Vector2(.23f+i*.25f,.98f),new Color(1,.84f,.08f));int n=v.currentVertCount;v.AddVert(P(new Vector2(.41f,.26f)),Color.yellow,Vector2.zero);v.AddVert(P(new Vector2(.41f,.60f)),Color.yellow,Vector2.zero);v.AddVert(P(new Vector2(.70f,.43f)),Color.yellow,Vector2.zero);v.AddTriangle(n,n+1,n+2);}
Vector3 P(Vector2 p){var r=rectTransform.rect;return new Vector3(r.xMin+p.x*r.width,r.yMin+p.y*r.height);}
void Quad(VertexHelper v,float x,float y,float X,float Y,Color c){Poly(v,new Vector2(x,y),new Vector2(X,y),new Vector2(X,Y),new Vector2(x,Y),c);}
void Poly(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color col){int n=v.currentVertCount;v.AddVert(P(a),col,Vector2.zero);v.AddVert(P(b),col,Vector2.zero);v.AddVert(P(c),col,Vector2.zero);v.AddVert(P(d),col,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
}}