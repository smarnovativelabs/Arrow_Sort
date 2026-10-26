using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolMenuSymbol:MaskableGraphic {
 public enum Kind{Gear,Music,Sound}public Kind kind;
 void Quad(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color col){int n=v.currentVertCount;v.AddVert(a,col,Vector2.zero);v.AddVert(b,col,Vector2.zero);v.AddVert(c,col,Vector2.zero);v.AddVert(d,col,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
 void Box(VertexHelper v,float x,float y,float w,float h,float s){Quad(v,new Vector2(x-w/2,y-h/2)*s,new Vector2(x-w/2,y+h/2)*s,new Vector2(x+w/2,y+h/2)*s,new Vector2(x+w/2,y-h/2)*s,color);}
 void Disc(VertexHelper v,Vector2 pos,float r,Color c){int n=v.currentVertCount;v.AddVert(pos,c,Vector2.zero);for(int i=0;i<=32;i++){float a=i*Mathf.PI/16;v.AddVert(pos+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,c,Vector2.zero);if(i>0)v.AddTriangle(n,n+i,n+i+1);}}
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();float s=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
  if(kind==Kind.Gear){for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),p=new Vector2(-d.y,d.x);Quad(v,(d*.52f-p*.15f)*s,(d*.85f-p*.15f)*s,(d*.85f+p*.15f)*s,(d*.52f+p*.15f)*s,color);}Disc(v,Vector2.zero,s*.65f,color);Disc(v,Vector2.zero,s*.27f,new Color(.06f,.25f,.45f));}
  else if(kind==Kind.Music){Box(v,-.2f,.12f,.13f,1.05f,s);Box(v,.56f,.3f,.13f,1.05f,s);Quad(v,new Vector2(-.26f,.5f)*s,new Vector2(-.26f,.75f)*s,new Vector2(.62f,.95f)*s,new Vector2(.62f,.7f)*s,color);Disc(v,new Vector2(-.38f,-.42f)*s,s*.26f,color);Disc(v,new Vector2(.39f,-.23f)*s,s*.26f,color);}
  else{Box(v,-.48f,0,.4f,.56f,s);Quad(v,new Vector2(-.4f,-.28f)*s,new Vector2(-.4f,.28f)*s,new Vector2(.06f,.65f)*s,new Vector2(.06f,-.65f)*s,color);for(int ring=0;ring<2;ring++)for(int i=0;i<14;i++){float a=(-55+i*110f/14)*Mathf.Deg2Rad,b=(-55+(i+1)*110f/14)*Mathf.Deg2Rad,r=.43f+ring*.25f;Vector2 p=new Vector2(.12f,0);Quad(v,(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r)*s,(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(r+.08f))*s,(p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(r+.08f))*s,(p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r)*s,color);}}
 }
}}
