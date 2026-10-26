using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
namespace WoolGame {
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolAbilityTileGraphic:MaskableGraphic {
 public Sprite tile;
 public override Texture mainTexture=>tile?tile.texture:Texture2D.whiteTexture;
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();if(!tile)return;var r=rectTransform.rect;var uv=DataUtility.GetOuterUV(tile);float radius=Mathf.Min(r.width,r.height)*.2386f;
  vh.AddVert(new Vector3(r.center.x,r.center.y),color,new Vector2((uv.x+uv.z)*.5f,(uv.y+uv.w)*.5f));
  for(int k=0;k<4;k++){float cx=k==0||k==3?radius:r.width-radius,cy=k<2?radius:r.height-radius;
   for(int j=0;j<9;j++){float a=(180+k*90+j*90f/8)*Mathf.Deg2Rad;var p=new Vector2(cx+Mathf.Cos(a)*radius,cy+Mathf.Sin(a)*radius);vh.AddVert(new Vector3(r.xMin+p.x,r.yMin+p.y),color,new Vector2(Mathf.Lerp(uv.x,uv.z,p.x/r.width),Mathf.Lerp(uv.y,uv.w,p.y/r.height)));}}
  for(int i=0;i<36;i++)vh.AddTriangle(0,1+i,1+(i+1)%36);
 }
}}
