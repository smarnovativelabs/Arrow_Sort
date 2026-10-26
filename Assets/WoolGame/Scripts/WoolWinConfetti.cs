using UnityEngine;using UnityEngine.UI;
namespace WoolGame {
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WoolWinConfetti:MaskableGraphic {
 float clock=8;static readonly Color[] palette={new Color(1,.72f,.08f),new Color(1,.22f,.3f),new Color(.2f,.78f,1),new Color(.35f,1,.5f),new Color(.85f,.4f,1),Color.white};
 public void Play(){clock=0;enabled=true;SetVerticesDirty();}
 void Update(){clock+=Mathf.Min(Time.unscaledDeltaTime,.05f);SetVerticesDirty();if(clock>=6)enabled=false;}
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();if(clock>=6)return;var r=rectTransform.rect;for(int i=0;i<72;i++){float t=clock-(i%12)*.035f;if(t<0)continue;float x=(Mathf.Repeat(i*.618034f,1)-.5f)*r.width*.95f+Mathf.Sin(t*2+i)*28;float y=r.height*.55f+(i%7)*15-(170+i%8*24)*t-45*t*t;if(y<r.yMin-30)continue;float a=t*(i%2==0?3:-3)+i,cs=Mathf.Cos(a),sn=Mathf.Sin(a),w=5+i%4,h=3+i%3;var c=palette[i%palette.Length];c.a=Mathf.Clamp01(5.5f-clock);int n=v.currentVertCount;for(int k=0;k<4;k++){float px=(k==0||k==3?-w:w),py=k<2?-h:h;v.AddVert(new Vector3(x+px*cs-py*sn,y+px*sn+py*cs),c,Vector2.zero);}v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}}
}}
