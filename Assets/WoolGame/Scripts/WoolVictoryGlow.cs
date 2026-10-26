using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
// A single UI mesh supplies the soft golden spotlight and slow celebration rays.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolVictoryGlow:MaskableGraphic {
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
  var center=rectTransform.rect.center;
  for(int i=0;i<80;i++){
   float a=i*Mathf.PI*2/80,b=(i+1)*Mathf.PI*2/80;
   int n=vh.currentVertCount;vh.AddVert(center,new Color(1,.65f,.12f,.42f),Vector2.zero);
   vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Color(1,.49f,.08f,0),Vector2.zero);
   vh.AddVert(center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,new Color(1,.49f,.08f,0),Vector2.zero);vh.AddTriangle(n,n+1,n+2);
  }
  for(int i=0;i<16;i++){
   float a=i*Mathf.PI*2/16;int n=vh.currentVertCount;
   vh.AddVert(center,new Color(1,.78f,.24f,.34f),Vector2.zero);
   vh.AddVert(center+new Vector2(Mathf.Cos(a-.055f),Mathf.Sin(a-.055f))*radius,new Color(1,.7f,.18f,0),Vector2.zero);
   vh.AddVert(center+new Vector2(Mathf.Cos(a+.055f),Mathf.Sin(a+.055f))*radius,new Color(1,.7f,.18f,0),Vector2.zero);vh.AddTriangle(n,n+1,n+2);
  }
 }
}}
