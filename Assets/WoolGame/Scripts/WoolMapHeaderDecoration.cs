using UnityEngine;using UnityEngine.UI;
namespace WoolGame{
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolMapHeaderDecoration:MaskableGraphic{
 public bool star;public bool left;
 void Poly(VertexHelper v,Vector2[] points,Color center,Color edge){int n=v.currentVertCount;v.AddVert(Vector2.zero,center,Vector2.zero);foreach(var p in points)v.AddVert(p,edge,Vector2.zero);for(int i=0;i<points.Length;i++)v.AddTriangle(n,n+1+i,n+1+(i+1)%points.Length);}
 void Disc(VertexHelper v,float radius,Color c){var p=new Vector2[64];for(int i=0;i<p.Length;i++){float a=i*Mathf.PI*2/p.Length;p[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;}Poly(v,p,Color.Lerp(c,Color.white,.15f),c);}
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();float r=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.48f;if(star){var p=new Vector2[10];for(int i=0;i<10;i++){float a=(90+i*36)*Mathf.Deg2Rad;p[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*(i%2==0?1:.48f);}Poly(v,p,WoolArt.Hex("FFF35A"),WoolArt.Hex("F5AA0B"));return;}Disc(v,r,WoolArt.Hex("729F9E"));Disc(v,r*.91f,WoolArt.Hex("D7EEE5"));float s=left?-1:1;int n=v.currentVertCount;v.AddVert(new Vector2(-s*r*.2f,-r*.43f),WoolArt.Hex("316C72"),Vector2.zero);v.AddVert(new Vector2(s*r*.35f,0),WoolArt.Hex("316C72"),Vector2.zero);v.AddVert(new Vector2(-s*r*.2f,r*.43f),WoolArt.Hex("316C72"),Vector2.zero);v.AddTriangle(n,n+1,n+2);}
}}
