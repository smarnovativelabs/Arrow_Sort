using UnityEngine;using UnityEngine.UI;
namespace WoolGame{
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolSpeedButtonFace:MaskableGraphic{
 public bool active;
 void Round(VertexHelper v,float inset,float drop,Color top,Color bottom){float w=rectTransform.rect.width-inset*2,h=rectTransform.rect.height-inset*2;float radius=Mathf.Min(h*.42f,28);int center=v.currentVertCount;v.AddVert(new Vector2(0,drop),Color.Lerp(top,bottom,.5f),Vector2.zero);for(int c=0;c<4;c++)for(int j=0;j<=20;j++){float angle=(c*90+j*4.5f)*Mathf.Deg2Rad;float x=Mathf.Cos(angle),y=Mathf.Sin(angle);var p=new Vector2(Mathf.Sign(x)*(w*.5f-radius)+x*radius,Mathf.Sign(y)*(h*.5f-radius)+y*radius+drop);v.AddVert(p,Color.Lerp(bottom,top,Mathf.Clamp01((p.y-drop)/h+.5f)),Vector2.zero);}for(int i=0;i<84;i++)v.AddTriangle(center,center+1+i,center+1+(i+1)%84);}
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();Round(v,0,-4,WoolArt.Hex("DF8015"),WoolArt.Hex("A65308"));Round(v,1,0,WoolArt.Hex("FFF0A6"),WoolArt.Hex("F4AC23"));Round(v,5,1,WoolArt.Hex("FFDB52"),WoolArt.Hex("DD8312"));Round(v,8,2,active?WoolArt.Hex("D69A2F"):WoolArt.Hex("FFCF35"),active?WoolArt.Hex("B97413"):WoolArt.Hex("F5A719"));}
}}
