using UnityEngine;
using UnityEngine.UI;
namespace WoolGame {
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolRailGuideTail:MaskableGraphic {
 public RectTransform bubble;public Vector2 tip;
 protected override void OnPopulateMesh(VertexHelper mesh){mesh.Clear();if(!bubble)return;Vector2 top=bubble.anchoredPosition-Vector2.up*(bubble.rect.height*.5f-8);Add(mesh,tip,top+Vector2.left*32,top+Vector2.right*32,WoolArt.Hex("FFC348"));Add(mesh,Vector2.Lerp(tip,top,.16f),top+Vector2.left*23,top+Vector2.right*23,WoolArt.Hex("FFF4D9"));}
 static void Add(VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Color color){int start=mesh.currentVertCount;mesh.AddVert(a,color,Vector2.zero);mesh.AddVert(b,color,Vector2.zero);mesh.AddVert(c,color,Vector2.zero);mesh.AddTriangle(start,start+1,start+2);}
}
}
