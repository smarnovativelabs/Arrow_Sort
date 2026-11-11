using UnityEngine;
using UnityEngine.UI;
namespace WoolGame{
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolRewardVideoIcon:MaskableGraphic{
 void Quad(VertexHelper v,float x,float y,float w,float h,Color c){int n=v.currentVertCount;v.AddVert(new Vector2(x,y),c,Vector2.zero);v.AddVert(new Vector2(x,y+h),c,Vector2.zero);v.AddVert(new Vector2(x+w,y+h),c,Vector2.zero);v.AddVert(new Vector2(x+w,y),c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height;var gold=WoolArt.Hex("F6B94E");var ink=WoolArt.Hex("493D62");var cream=WoolArt.Hex("FFF5D9");Quad(v,-w*.5f,-h*.5f,w,h*.76f,gold);Quad(v,-w*.44f,-h*.43f,w*.88f,h*.6f,ink);Quad(v,-w*.5f,h*.28f,w,h*.2f,ink);for(int i=0;i<4;i++)Quad(v,-w*.45f+i*w*.25f,h*.29f,w*.12f,h*.18f,cream);int n=v.currentVertCount;v.AddVert(new Vector2(-w*.12f,-h*.31f),cream,Vector2.zero);v.AddVert(new Vector2(-w*.12f,h*.07f),cream,Vector2.zero);v.AddVert(new Vector2(w*.2f,-h*.12f),cream,Vector2.zero);v.AddTriangle(n,n+1,n+2);}
}}
